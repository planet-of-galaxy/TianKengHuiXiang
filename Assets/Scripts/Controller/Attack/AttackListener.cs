using QFramework;
using UnityEngine;

/// <summary>
/// 攻击输入监听：挂在玩家/角色物体上，每帧检测鼠标左键单击，
/// 触发时向架构发送 <see cref="PlayerAttackEvent"/>。
/// 监听该事件的 WeaponAttackBase 等组件负责执行实际攻击，从而将“输入”与“攻击逻辑”解耦。
/// </summary>
public class AttackListener : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Update()
    {
        // 左键单击（按下的那一帧触发一次）
        if (Input.GetMouseButtonDown(0))
        {
            GetArchitecture().SendEvent(new PlayerAttackEvent());
        }
    }
}
