using QFramework;
using UnityEngine;

/// <summary>
/// 拾取物控制器（参考 DoorController / InteractableBaseA 的交互模式）：
/// 玩家进入 TriggerController 触发区并聚焦 billboard 后按交互键（HotKeyUtility.InteractA，默认 E），
/// 将本物体配置的道具（itemType + configId）加入当前角色的背包（仅内存，不落盘）。
/// 交互触发由 InteractSystem 统一调度，与开门共用同一套触发/提示结构（Tigger + Canvas/Billboard）。
/// </summary>
public class PickupController : InteractableBaseA
{
    [Header("拾取道具")]
    [Tooltip("道具类型：目前仅支持 Weapon")]
    [SerializeField] private ItemType itemType;

    [Tooltip("对应配置表中的配置 id，例如武器 id（WeaponConfig.json 的 weaponId）")]
    [SerializeField] private int configId;

    [Tooltip("拾取成功后是否销毁本物体（测试用，默认 true）")]
    [SerializeField] private bool destroyOnPickup = true;

    public override void InteractA()
    {
        var controllingRole = this.GetSystem<IRoleInstanceSystem>().ControllingRole;
        if (controllingRole == null)
        {
            Debug.LogWarning("[PickupController] 当前没有受控角色，无法拾取");
            return;
        }

        int roleId = controllingRole.RoleRuntimeIndex;
        if (this.GetSystem<IPackageSystem>().AddItemToRolePackage(roleId, itemType, configId)
            && destroyOnPickup)
        {
            // 先从 InteractSystem 注销，避免销毁后仍作为交互目标被遍历
            StopListening();
            Destroy(gameObject);
        }
    }
}
