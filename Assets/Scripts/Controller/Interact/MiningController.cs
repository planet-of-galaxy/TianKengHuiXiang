using QFramework;
using UnityEngine;

/// <summary>沿用范围检测与聚焦提示，仅负责请求进入挖矿状态。</summary>
public class MiningController : InteractableBaseA
{
    [SerializeField, Min(1)] private int mineId = 1;

    public int MineId => mineId;

    private void Update()
    {
        UpdateInteractionAvailability();
    }

    private void UpdateInteractionAvailability()
    {
        var role = this.GetSystem<IRoleInstanceSystem>().ControllingRole;
        var state = role != null ? role.PlayerStateMachine?.StateMachine.CurrentDeepState as PlayerMineState : null;
        var isMiningThisOre = state?.Context != null
            && state.Context.MineId == mineId
            && state.Context.Position == transform.position;
        SetInteractionEnabled(!isMiningThisOre);
    }

    public override void InteractA()
    {
        if (this.GetSystem<IGamePauseSystem>().IsPaused) return;

        var role = this.GetSystem<IRoleInstanceSystem>().ControllingRole;
        var player = role != null ? role.PlayerStateMachine : null;
        if (player == null) return;

        if (player.StateMachine.IsInState<PlayerMineState>()) return;

        // 状态机先处理退出时，同一帧的交互输入不能再次进入挖矿。
        if (player.StateMachine.PreviousState is PlayerMineState previousMine
            && previousMine.LastExitFrame == Time.frameCount) return;

        player.ChangeState<PlayerMineState>(new MineContext(mineId, transform.position));
        UpdateInteractionAvailability();
    }
}
