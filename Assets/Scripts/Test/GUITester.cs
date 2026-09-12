using QFramework;
using UnityEngine;

/// <summary>进入 TestState 时自动挂载；跨场景保留，按 Tab 显示或隐藏调试面板。</summary>
[DisallowMultipleComponent]
public class GUITester : MonoBehaviour, IController
{
    private static GUITester instance;
    private bool visible;
    private CursorUtility.CursorSnapshot cursorBeforeOpen;
    private string configIdText = "0";
    private string slotIndexText = "0";
    private string message = "按 Tab 显示或隐藏。背包槽位从 0 开始。";

    /// <summary>仅在没有常驻实例时创建，重复进入 TestState 不会重复挂载。</summary>
    public static void EnsureCreated()
    {
        if (instance != null) return;
        new GameObject(nameof(GUITester)).AddComponent<GUITester>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (instance == this && Input.GetKeyDown(KeyCode.Tab)) SetVisible(!visible);
    }

    private void OnDisable()
    {
        if (instance == this) SetVisible(false);
    }

    private void SetVisible(bool show)
    {
        if (visible == show) return;

        if (show)
        {
            cursorBeforeOpen = CursorUtility.Capture();
            CursorUtility.ShowAndUnlock();
        }
        else
        {
            cursorBeforeOpen.Restore();
        }

        visible = show;
    }

    private void OnGUI()
    {
        if (!visible || instance != this) return;

        GUILayout.BeginArea(new Rect(20, 20, 420, 260), GUI.skin.box);
        GUILayout.Label("GUITester（Tab 隐藏）");
        GUILayout.BeginHorizontal();
        GUILayout.Label("武器配置 ID", GUILayout.Width(130));
        configIdText = GUILayout.TextField(configIdText, 10);
        if (GUILayout.Button("添加", GUILayout.Width(90))) AddItem();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("背包槽位（从 0 起）", GUILayout.Width(130));
        slotIndexText = GUILayout.TextField(slotIndexText, 10);
        if (GUILayout.Button("手持", GUILayout.Width(90))) HoldItem();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("打印武器")) PrintWeapon();
        GUILayout.Label(message);
        GUILayout.EndArea();
    }

    private void AddItem()
    {
        if (!TryReadNumber(configIdText, "配置 ID", out int configId) ||
            !TryGetCurrentRole(out int roleId)) return;

        bool added = this.GetSystem<IPackageSystem>()
            .AddItemToRolePackage(roleId, ItemType.Weapon, configId);
        Report(added ? $"已将武器 {configId} 添加到角色 {roleId} 的背包。" :
            "添加失败，请检查配置 ID、背包容量及 Console 日志。", !added);
    }

    private void HoldItem()
    {
        if (!TryReadNumber(slotIndexText, "背包槽位", out int slotIndex) ||
            !TryGetCurrentPackage(out var package)) return;

        foreach (var item in package.Items)
        {
            if (item == null || item.index != slotIndex) continue;
            package.heldIndex.Value = item.index;
            Report($"角色 {package.roleRuntimeId} 已手持槽位 {slotIndex} 的物品（配置 ID：{item.configId}）。");
            return;
        }

        Report($"背包槽位 {slotIndex} 中没有物品。", true);
    }

    private void PrintWeapon()
    {
        if (!TryGetCurrentPackage(out var package)) return;

        foreach (var item in package.Items)
        {
            if (item == null || item.index != package.heldIndex.Value) continue;
            if (!(item is WeaponItemInfo weapon))
            {
                Report("当前手持物品不是武器。", true);
                return;
            }

            var config = this.GetUtility<IWeaponConfigProvider>().GetWeaponConfig(weapon.configId);
            string properties = $"角色：{package.roleRuntimeId}\n槽位：{weapon.index}\n" +
                $"配置 ID：{weapon.configId}\n类型：{weapon.Type}\n" +
                $"数量：{weapon.num?.Value}\n当前耐久：{weapon.durability?.Value}\n";
            // 输出完整配置，包含满耐久以及轻、重攻击的全部属性。
            Debug.Log("[GUITester] 当前武器属性\n" + properties +
                (config == null ? "未找到武器配置。" : JsonUtility.ToJson(config, true)), this);
            Report(config == null ? "已打印运行时属性，但未找到武器配置。" :
                "当前武器的完整属性已打印到 Console。", config == null);
            return;
        }

        Report("当前没有手持物品，或手持槽位中没有物品。", true);
    }

    private bool TryGetCurrentRole(out int roleId)
    {
        var model = this.GetModel<RoleRuntimeModel>();
        roleId = this.GetModel<RoleInstanceModel>().curRole.Value;
        if (roleId >= 0 && model.TryGetRoleRuntime(roleId, out _)) return true;
        Report("当前没有有效角色，请先选择角色。", true);
        return false;
    }

    private bool TryGetCurrentPackage(out RolePackageInfo package)
    {
        package = null;
        if (!TryGetCurrentRole(out int roleId)) return false;
        if (this.GetModel<PackageModel>().TryGetPackage(roleId, out package)) return true;
        Report($"角色 {roleId} 尚未创建背包。", true);
        return false;
    }

    private bool TryReadNumber(string text, string label, out int value)
    {
        if (int.TryParse(text, out value) && value >= 0) return true;
        Report($"{label} 必须是非负整数。", true);
        return false;
    }

    private void Report(string text, bool warning = false)
    {
        message = text;
        if (warning) Debug.LogWarning("[GUITester] " + text, this);
        else Debug.Log("[GUITester] " + text, this);
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
