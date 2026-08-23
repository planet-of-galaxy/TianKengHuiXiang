using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包面板（UIKit 管理）：由 PackageController 通过 UIKit.OpenPanel 加载。
/// 始终绘制 PackageSystem.maxCapacity 个栏位：
///   index &lt; capacity                     → 白色（可用空间）
///   capacity ≤ index &lt; maxCapacity      → 灰色（已达上限但尚未解锁）
/// OnInit 订阅 PackageModel.capacity，容量变化时自动刷新栏位颜色。
/// </summary>
public class PackagePanel : UIPanel, IController
{
    [Header("布局")]
    [SerializeField] private int columns = 8;
    [SerializeField] private float slotSize = 64f;
    [SerializeField] private float spacing = 8f;
    [SerializeField] private float padding = 24f;

    [Header("颜色")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    private readonly List<Image> slots = new List<Image>();
    private PackageModel packageModel;
    private IUnRegister capacityUnRegister;

    /// <summary>总栏位数：永远显示到 maxCapacity。</summary>
    private int TotalSlots => Mathf.Max(1, PackageSystem.maxCapacity);

    /// <summary>当前可用容量：运行态取模型值，未取到模型时用默认容量。</summary>
    private int Capacity
    {
        get
        {
            if (Application.isPlaying && packageModel != null)
                return packageModel.capacity.Value;
            return PackageSystem.defaultCapacity;
        }
    }

    protected override void OnInit(IUIData uiData = null)
    {
        packageModel = this.GetModel<PackageModel>();

        capacityUnRegister?.UnRegister();
        capacityUnRegister = packageModel.capacity.Register(OnCapacityChanged);

        RebuildSlots();
    }

    protected override void OnClose()
    {
        capacityUnRegister?.UnRegister();
        capacityUnRegister = null;
    }

    private void OnCapacityChanged(int capacity)
    {
        ApplySlotColors();
    }

    /// <summary>构建（或重建）背景与全部栏位，然后按容量上色。</summary>
    private void RebuildSlots()
    {
        slots.Clear();

        // 清理旧的构建产物，保证重复调用幂等
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name == "Grid" || child.name.StartsWith("Slot"))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // 背景：自身加半透明底
        var bg = GetComponent<Image>();
        if (bg == null)
        {
            bg = gameObject.AddComponent<Image>();
            bg.raycastTarget = false;
        }
        bg.color = new Color(0f, 0f, 0f, 0.55f);

        // 计算网格尺寸并调整面板大小
        int total = TotalSlots;
        int rows = Mathf.CeilToInt(total / (float)columns);
        float gridW = columns * slotSize + (columns - 1) * spacing;
        float gridH = rows * slotSize + (rows - 1) * spacing;

        var rect = transform as RectTransform;
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(gridW + padding * 2, gridH + padding * 2);
        }

        // 网格容器：拉伸到面板内缩 padding，GridLayoutGroup 排布栏位
        var gridGO = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGO.transform.SetParent(transform, false);

        var gridRect = gridGO.transform as RectTransform;
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.offsetMin = new Vector2(padding, padding);
        gridRect.offsetMax = new Vector2(-padding, -padding);

        var grid = gridGO.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(slotSize, slotSize);
        grid.spacing = new Vector2(spacing, spacing);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        // 栏位
        for (int i = 0; i < total; i++)
        {
            var slotGO = new GameObject("Slot_" + i, typeof(RectTransform), typeof(Image));
            slotGO.transform.SetParent(gridGO.transform, false);

            var img = slotGO.GetComponent<Image>();
            img.raycastTarget = false;
            slots.Add(img);
        }

        ApplySlotColors();
    }

    /// <summary>按当前容量刷新栏位颜色：可用=白，未解锁=灰。</summary>
    private void ApplySlotColors()
    {
        int capacity = Capacity;
        int total = TotalSlots;

        for (int i = 0; i < slots.Count && i < total; i++)
        {
            slots[i].color = i < capacity ? availableColor : lockedColor;
        }
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
