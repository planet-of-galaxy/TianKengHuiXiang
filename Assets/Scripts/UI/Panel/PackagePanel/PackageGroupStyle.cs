using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包面板分组布局：Key / File / Prop 三个 Panel 在 Main / Left / Right 三个占位点之间轮换。
/// 初始布局：PropPanel 占 MainViewTrans，KeyPanel 占 LeftViewTrans，FilePanel 占 RightViewTrans。
/// 点击按钮时，该 Panel 与当前占据 MainViewTrans 的 Panel 交换位置；
/// 占据 MainViewTrans 的 Panel 会被置为最后一个 sibling（绘制在最上层），其按钮同时置为不可交互，避免误触。
/// 位置与尺寸用协程插值，不会瞬移；对齐连尺寸一起套用占位点的 RectTransform，
/// 所以退到两侧的 Panel 会平滑收窄成 Left/RightViewTrans 的大小。
/// </summary>
public class PackageGroupStyle : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private RectTransform KeyPanel;
    [SerializeField] private RectTransform FilePanel;
    [SerializeField] private RectTransform PropPanel;

    [Header("按钮")]
    [SerializeField] private Button KeyButton;
    [SerializeField] private Button FileButton;
    [SerializeField] private Button PropButton;

    [Header("占位点")]
    [SerializeField] private RectTransform MainViewTrans;
    [SerializeField] private RectTransform LeftViewTrans;
    [SerializeField] private RectTransform RightViewTrans;

    [Header("动画")]
    [Tooltip("切换过渡时长（秒）。用 unscaledTime 计时，暂停中也能正常播放。")]
    [SerializeField] private float transitionDuration = 0.25f;

    /// <summary>Panel → 当前占据的占位点。</summary>
    private readonly Dictionary<RectTransform, RectTransform> panelSpots = new Dictionary<RectTransform, RectTransform>();

    /// <summary>Panel → 触发切换的按钮。</summary>
    private readonly Dictionary<RectTransform, Button> panelButtons = new Dictionary<RectTransform, Button>();

    /// <summary>Panel → 正在播放的过渡协程，重复点击时先停掉再从当前位置重新插值。</summary>
    private readonly Dictionary<RectTransform, Coroutine> runningTweens = new Dictionary<RectTransform, Coroutine>();

    private void Awake()
    {
        if (KeyButton != null)
        {
            KeyButton.onClick.AddListener(() => SwitchTo(KeyPanel));
        }

        if (FileButton != null)
        {
            FileButton.onClick.AddListener(() => SwitchTo(FilePanel));
        }

        if (PropButton != null)
        {
            PropButton.onClick.AddListener(() => SwitchTo(PropPanel));
        }

        ResetLayout();
    }

    /// <summary>瞬间回到初始布局：物品页在主视图，键位页在左，文件页在右。编辑器里可通过右键菜单预览。</summary>
    [ContextMenu("重置为初始布局")]
    public void ResetLayout()
    {
        panelSpots.Clear();
        Add(panelSpots, PropPanel, MainViewTrans);
        Add(panelSpots, KeyPanel, LeftViewTrans);
        Add(panelSpots, FilePanel, RightViewTrans);

        panelButtons.Clear();
        Add(panelButtons, KeyPanel, KeyButton);
        Add(panelButtons, FilePanel, FileButton);
        Add(panelButtons, PropPanel, PropButton);

        foreach (var pair in panelSpots)
        {
            SnapTo(pair.Key, pair.Value);
        }

        RefreshButtons();
    }

    /// <summary>把 panel 切到主视图，原本占据主视图的 Panel 退到 panel 原来的位置，两个 Panel 同时平滑过渡。</summary>
    public void SwitchTo(RectTransform panel)
    {
        if (panel == null || !panelSpots.TryGetValue(panel, out var oldSpot))
        {
            return;
        }

        // 已经在主视图：直接忽略（此时其按钮也是禁用的）
        if (oldSpot == MainViewTrans)
        {
            return;
        }

        var mainPanel = FindPanelAt(MainViewTrans);

        panelSpots[panel] = MainViewTrans;
        MoveTo(panel, MainViewTrans);

        if (mainPanel != null)
        {
            panelSpots[mainPanel] = oldSpot;
            MoveTo(mainPanel, oldSpot);
        }

        RefreshButtons();
    }

    /// <summary>刷新按钮状态：占据主视图的 Panel，其按钮置为不可交互。</summary>
    private void RefreshButtons()
    {
        foreach (var pair in panelButtons)
        {
            if (!panelSpots.TryGetValue(pair.Key, out var spot))
            {
                continue;
            }

            pair.Value.interactable = spot != MainViewTrans;
        }
    }

    private RectTransform FindPanelAt(RectTransform spot)
    {
        foreach (var pair in panelSpots)
        {
            if (pair.Value == spot)
            {
                return pair.Key;
            }
        }

        return null;
    }

    /// <summary>把 Panel 平滑过渡到占位点；该 Panel 若正在过渡，则从当前位置重新开始，避免两次动画打架。</summary>
    private void MoveTo(RectTransform panel, RectTransform spot)
    {
        if (panel == null || spot == null)
        {
            return;
        }

        if (runningTweens.TryGetValue(panel, out var running) && running != null)
        {
            StopCoroutine(running);
        }

        runningTweens[panel] = StartCoroutine(MoveRoutine(panel, spot));
    }

    /// <summary>
    /// 位置与尺寸插值到占位点。锚点 / 轴心 / 缩放 / 旋转三个占位点完全一致，不需要插值，直接套用。
    /// 计时用 unscaledDeltaTime：背包面板打开时 GamePauseSystem 会把 timeScale 置 0，用 deltaTime 会一直卡住。
    /// </summary>
    private IEnumerator MoveRoutine(RectTransform panel, RectTransform spot)
    {
        panel.anchorMin = spot.anchorMin;
        panel.anchorMax = spot.anchorMax;
        panel.pivot = spot.pivot;
        panel.localRotation = spot.localRotation;
        panel.localScale = spot.localScale;

        // 进入主视图的 Panel 立即置顶，过渡过程中就压在最上层
        if (spot == MainViewTrans)
        {
            panel.SetAsLastSibling();
        }

        var fromPosition = panel.anchoredPosition3D;
        var toPosition = spot.anchoredPosition3D;
        var fromSize = panel.sizeDelta;
        var toSize = spot.sizeDelta;

        if (transitionDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                // SmoothStep 做缓入缓出，避免匀速移动的生硬感
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
                panel.anchoredPosition3D = Vector3.LerpUnclamped(fromPosition, toPosition, t);
                panel.sizeDelta = Vector2.LerpUnclamped(fromSize, toSize, t);

                yield return null;
            }
        }

        // 收尾对齐，避免浮点误差留下零头
        panel.anchoredPosition3D = toPosition;
        panel.sizeDelta = toSize;
        runningTweens.Remove(panel);
    }

    /// <summary>不做过渡，直接把 panel 的 RectTransform 对齐到占位点（含尺寸）；占据主视图的一并置顶。</summary>
    private void SnapTo(RectTransform panel, RectTransform spot)
    {
        if (panel == null || spot == null)
        {
            return;
        }

        panel.anchorMin = spot.anchorMin;
        panel.anchorMax = spot.anchorMax;
        panel.pivot = spot.pivot;
        panel.sizeDelta = spot.sizeDelta;
        panel.anchoredPosition3D = spot.anchoredPosition3D;
        panel.localRotation = spot.localRotation;
        panel.localScale = spot.localScale;

        if (spot == MainViewTrans)
        {
            panel.SetAsLastSibling();
        }
    }

    /// <summary>登记 Panel → 占位点；任一侧未配置时跳过，避免运行时抛空引用。</summary>
    private static void Add(Dictionary<RectTransform, RectTransform> map, RectTransform panel, RectTransform spot)
    {
        if (panel != null && spot != null)
        {
            map[panel] = spot;
        }
    }

    /// <summary>登记 Panel → 按钮；任一侧未配置时跳过。</summary>
    private static void Add(Dictionary<RectTransform, Button> map, RectTransform panel, Button button)
    {
        if (panel != null && button != null)
        {
            map[panel] = button;
        }
    }
}
