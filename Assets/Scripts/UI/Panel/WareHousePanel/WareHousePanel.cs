using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 仓库面板（UIKit 管理）：由 WareHouseController 通过 UIKit.OpenPanel 加载。
/// 左侧列举 WareHouse 中所有道具，右侧展示选中角色（默认当前角色）的背包道具，
/// 顶部通过人物按钮切换右侧展示哪个角色的背包。
/// 说明：当前为纯展示面板（无道具搬运交互），文字列表、暂不渲染图标。
/// 布局由代码动态生成，预制体只需一个空 UIPanel 根节点。
/// </summary>
public class WareHousePanel : UIPanel, IController
{
    [Header("布局")]
    [SerializeField] private float topBarHeight = 64f;
    [SerializeField] private float areaGap = 16f;
    [SerializeField] private float padding = 16f;
    [SerializeField] private float itemRowHeight = 32f;

    [Header("颜色")]
    [SerializeField] private Color panelBgColor = new Color(0f, 0f, 0f, 0.65f);
    [SerializeField] private Color titleColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color rowColor = new Color(1f, 1f, 1f, 0.06f);
    [SerializeField] private Color buttonColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color buttonActiveColor = new Color(0.15f, 0.45f, 0.25f, 1f);

    private PackageModel packageModel;
    private RoleRuntimeModel roleRuntimeModel;
    private WareHouseModel wareHouseModel;
    private IWeaponConfigProvider weaponConfigProvider;

    /// <summary>右侧当前展示的角色运行时实例 id；默认跟随当前角色，点击人物按钮后改为本地选择。</summary>
    private int displayedRoleId = -1;
    private bool manualPicked;

    private RectTransform roleBarContent;
    private RectTransform wareListContent;
    private RectTransform packListContent;
    private Text rightTitleText;

    private readonly Dictionary<int, Image> roleButtonImages = new Dictionary<int, Image>();

    /// <summary>缓存一张 1x1 白色 Sprite，供动态生成的 Image 使用。</summary>
    private static Sprite _whiteSprite;

    private static Sprite WhiteSprite
    {
        get
        {
            if (_whiteSprite == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            }
            return _whiteSprite;
        }
    }

    protected override void OnInit(IUIData uiData = null)
    {
        packageModel = this.GetModel<PackageModel>();
        roleRuntimeModel = this.GetModel<RoleRuntimeModel>();
        wareHouseModel = this.GetModel<WareHouseModel>();
        weaponConfigProvider = this.GetUtility<IWeaponConfigProvider>();

        // 面板根节点铺满 UIKit 根节点
        var rect = transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        BuildLayout();
    }

    protected override void OnShow()
    {
        // 每次打开都回到“默认跟随当前角色”，点击人物按钮可本地切换
        manualPicked = false;
        RefreshAll();
    }

    protected override void OnClose()
    {
    }

    // ==================== 布局构建（仅一次） ====================

    private void BuildLayout()
    {
        var bg = AddImage(transform, "Bg", panelBgColor);
        StretchFill(bg.rectTransform, 0f);

        BuildRoleBar();
        BuildLeftWareList();
        BuildRightPackageList();
    }

    /// <summary>顶部角色选择栏：标题 + 水平排布的人物按钮容器。</summary>
    private void BuildRoleBar()
    {
        var bar = CreateRect("RoleBar", transform);
        bar.anchorMin = new Vector2(0f, 1f);
        bar.anchorMax = new Vector2(1f, 1f);
        bar.pivot = new Vector2(0.5f, 1f);
        bar.anchoredPosition = new Vector2(0f, -padding);
        bar.sizeDelta = new Vector2(0f, topBarHeight);

        var label = CreateText(bar, "RoleBarLabel", "选择角色：", titleColor, 26);
        label.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        label.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        label.rectTransform.pivot = new Vector2(0f, 0.5f);
        label.rectTransform.anchoredPosition = new Vector2(0f, 0f);
        label.rectTransform.sizeDelta = new Vector2(140f, topBarHeight);
        label.alignment = TextAnchor.MiddleLeft;

        var barContentGO = new GameObject("RoleButtons", typeof(RectTransform));
        barContentGO.transform.SetParent(bar, false);
        roleBarContent = barContentGO.transform as RectTransform;
        roleBarContent.anchorMin = new Vector2(0f, 0.5f);
        roleBarContent.anchorMax = new Vector2(0f, 0.5f);
        roleBarContent.pivot = new Vector2(0f, 0.5f);
        roleBarContent.anchoredPosition = new Vector2(150f, 0f);
        roleBarContent.sizeDelta = new Vector2(0f, topBarHeight);

        var hlg = roleBarContent.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var csf = roleBarContent.gameObject.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private void BuildLeftWareList()
    {
        var area = CreateArea("WareArea", 0f, 0.5f);
        var title = CreateText(area, "Title", "仓库道具", titleColor, 28);
        StretchTop(title.rectTransform, 32f);

        var listRoot = CreateScrollList(area, out _, out wareListContent);
        StretchFillBelow(listRoot, 36f);
    }

    private void BuildRightPackageList()
    {
        var area = CreateArea("PackArea", 0.5f, 1f);
        rightTitleText = CreateText(area, "Title", "", titleColor, 28);
        StretchTop(rightTitleText.rectTransform, 32f);

        var listRoot = CreateScrollList(area, out _, out packListContent);
        StretchFillBelow(listRoot, 36f);
    }

    /// <summary>左右内容区：anchorMinX~anchorMaxX 铺满，顶部预留角色栏高度。</summary>
    private RectTransform CreateArea(string name, float anchorMinX, float anchorMaxX)
    {
        var rect = CreateRect(name, transform);
        rect.anchorMin = new Vector2(anchorMinX, 0f);
        rect.anchorMax = new Vector2(anchorMaxX, 1f);
        rect.offsetMin = new Vector2(anchorMinX == 0f ? padding : areaGap * 0.5f, padding);
        rect.offsetMax = new Vector2(anchorMaxX == 1f ? -padding : -areaGap * 0.5f, -(padding + topBarHeight + padding));
        return rect;
    }

    // ==================== 内容刷新 ====================

    private void RefreshAll()
    {
        var eligible = GetEligibleRoles();

        // 默认展示当前角色背包；当前角色无背包或不可用时取第一个有背包的角色
        if (!manualPicked)
        {
            displayedRoleId = -1;
            if (roleRuntimeModel.TryGetRoleRuntime(roleRuntimeModel.curRole.Value, out var cur)
                && packageModel.TryGetPackage(cur.runtimeIndex, out _))
            {
                displayedRoleId = cur.runtimeIndex;
            }

            if (displayedRoleId < 0 && eligible.Count > 0)
            {
                displayedRoleId = eligible[0].runtimeIndex;
            }
        }

        RefreshRoleBar(eligible);
        RefreshWareList();
        RefreshPackageList();
    }

    private void RefreshRoleBar(List<RoleRuntimeInfo> eligible)
    {
        ClearChildren(roleBarContent);
        roleButtonImages.Clear();

        if (eligible.Count == 0)
        {
            var text = CreateText(roleBarContent, "Empty", "（无角色）", titleColor, 24);
            text.alignment = TextAnchor.MiddleLeft;
            return;
        }

        foreach (var info in eligible)
        {
            roleButtonImages[info.runtimeIndex] = CreateRoleButton(info);
        }

        UpdateRoleHighlight();
    }

    private Image CreateRoleButton(RoleRuntimeInfo info)
    {
        var go = new GameObject("Role_" + info.runtimeIndex, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(roleBarContent, false);

        var layout = go.AddComponent<LayoutElement>();
        layout.preferredWidth = 120f;
        layout.preferredHeight = topBarHeight - 12f;

        var image = go.GetComponent<Image>();
        image.sprite = WhiteSprite;
        image.type = Image.Type.Sliced;

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;

        var label = CreateText(go.transform, "Name", info.name, Color.white, 24);
        StretchFill(label.rectTransform, 6f);
        label.alignment = TextAnchor.MiddleCenter;

        var capturedId = info.runtimeIndex;
        button.onClick.AddListener(() => OnRoleClicked(capturedId));
        return image;
    }

    private void OnRoleClicked(int roleRuntimeId)
    {
        manualPicked = true;
        displayedRoleId = roleRuntimeId;
        UpdateRoleHighlight();
        RefreshPackageList();
    }

    private void UpdateRoleHighlight()
    {
        foreach (var pair in roleButtonImages)
        {
            pair.Value.color = pair.Key == displayedRoleId ? buttonActiveColor : buttonColor;
        }
    }

    private List<RoleRuntimeInfo> GetEligibleRoles()
    {
        var roles = new List<RoleRuntimeInfo>();
        foreach (var info in roleRuntimeModel.GetAllRoleRuntimes())
        {
            if (packageModel.TryGetPackage(info.runtimeIndex, out _))
            {
                roles.Add(info);
            }
        }
        return roles;
    }

    // ==================== 道具列表 ====================

    private void RefreshWareList()
    {
        ClearChildren(wareListContent);

        var items = new List<PropItemInfo>(wareHouseModel.GetAllItems());
        if (items.Count == 0)
        {
            CreateListRow(wareListContent, "（仓库为空）", true);
            return;
        }

        foreach (var item in items)
        {
            if (item == null) continue;
            CreateListRow(wareListContent, DescribeWareItem(item), false);
        }
    }

    private void RefreshPackageList()
    {
        ClearChildren(packListContent);

        string rightTitle = "背包";
        if (roleRuntimeModel.TryGetRoleRuntime(displayedRoleId, out var info))
        {
            rightTitle = info.name + " 的背包";
        }
        if (rightTitleText != null)
        {
            rightTitleText.text = rightTitle;
        }

        if (!packageModel.TryGetPackage(displayedRoleId, out var package))
        {
            CreateListRow(packListContent, "（该角色暂无背包）", true);
            return;
        }

        var items = package.packageItems ?? new List<PropItemInfo>();
        var shown = false;
        foreach (var item in items)
        {
            if (item == null) continue;
            CreateListRow(packListContent, DescribePackageItem(item), false);
            shown = true;
        }

        if (!shown)
        {
            CreateListRow(packListContent, "（背包为空）", true);
        }
    }

    private void CreateListRow(Transform parent, string text, bool dim)
    {
        var row = new GameObject("ItemRow", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var layout = row.AddComponent<LayoutElement>();
        layout.preferredHeight = itemRowHeight;

        var bg = AddImage(row.transform, "Bg", rowColor);
        StretchFill(bg.rectTransform, 2f);

        var label = CreateText(row.transform, "Text", text, dim ? new Color(1f, 1f, 1f, 0.5f) : Color.white, 24);
        StretchFill(label.rectTransform, 6f);
        label.alignment = TextAnchor.MiddleLeft;
    }

    // ==================== 文案 ====================

    private string DescribeWareItem(PropItemInfo item)
    {
        var text = GetItemName(item);
        if (item is WeaponItemInfo weapon)
        {
            text += $"  (耐久 {weapon.durability?.Value ?? 0f:0})";
        }
        if (item.num != null && item.num.Value > 1)
        {
            text += $"  ×{item.num.Value}";
        }
        return text;
    }

    private string DescribePackageItem(PropItemInfo item)
    {
        var text = $"槽{item.index}  {GetItemName(item)}";
        if (item is WeaponItemInfo weapon)
        {
            text += $"  (耐久 {weapon.durability?.Value ?? 0f:0})";
        }
        if (item.num != null && item.num.Value > 1)
        {
            text += $"  ×{item.num.Value}";
        }
        return text;
    }

    private string GetItemName(PropItemInfo item)
    {
        var config = weaponConfigProvider.GetWeaponConfig(item.configId);
        return config != null ? config.name : $"道具{item.configId}";
    }

    // ==================== UI 工具 ====================

    private static void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.transform as RectTransform;
    }

    private static Image AddImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = WhiteSprite;
        image.color = color;
        return image;
    }

    private static Text CreateText(Transform parent, string name, string content, Color color, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = GetBuiltinFont();
        text.text = content;
        text.color = color;
        text.fontSize = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private static Font _builtinFont;

    private static Font GetBuiltinFont()
    {
        if (_builtinFont == null)
        {
            _builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_builtinFont == null)
            {
                _builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }
        return _builtinFont;
    }

    /// <summary>创建竖向滚动列表：area 内生成 Viewport(裁剪) + Content(自动增高)。</summary>
    private static RectTransform CreateScrollList(Transform parent, out ScrollRect scrollRect, out RectTransform content)
    {
        var root = CreateRect("ScrollList", parent);
        scrollRect = root.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        var viewport = CreateRect("Viewport", root);
        StretchFill(viewport, 0f);
        viewport.gameObject.AddComponent<RectMask2D>();
        scrollRect.viewport = viewport;

        content = CreateRect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 0f);
        scrollRect.content = content;

        var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4f;
        vlg.padding = new RectOffset(2, 8, 2, 2);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = content.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        return root;
    }

    // ==================== RectTransform 定位小工具 ====================

    private static void StretchFill(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private static void StretchTop(RectTransform rect, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, height);
    }

    private static void StretchFillBelow(RectTransform rect, float topInset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = new Vector2(0f, -topInset);
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
