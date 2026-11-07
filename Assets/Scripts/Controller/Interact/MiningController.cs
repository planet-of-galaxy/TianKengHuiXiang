using QFramework;
using UnityEngine;

/// <summary>沿用范围检测与聚焦提示，仅负责请求进入挖矿状态。</summary>
[RequireComponent(typeof(BoxCollider))]
public class MiningController : InteractableBaseA
{
    [SerializeField, Min(1)] private int mineId = 1;
    [SerializeField, Min(1)] private int initialGold = 4000;

    public int RemainingGold { get; internal set; } = -1;
    private IMiningSystem miningSystem;

    public int MineId => mineId;

    private void OnEnable()
    {
        miningSystem = this.GetSystem<IMiningSystem>();
        if (RemainingGold < 0) RemainingGold = initialGold;
        mineId = miningSystem.Register(this);
        FitCollisionBox();
    }

    private void OnDisable()
    {
        miningSystem?.Unregister(this);
        SetInteractionEnabled(false);
        StopListening();
    }

    internal void PrepareForRemoval()
    {
        SetInteractionEnabled(false);
        StopListening();
        enabled = false;
    }

    private void FitCollisionBox()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var bounds = new Bounds();
        bool hasBounds = false;
        foreach (var renderer in renderers)
        {
            if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
            var local = renderer.localBounds;
            for (int corner = 0; corner < 8; corner++)
            {
                var point = local.center + Vector3.Scale(local.extents, new Vector3(
                    (corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                point = transform.InverseTransformPoint(renderer.transform.TransformPoint(point));
                if (!hasBounds) { bounds = new Bounds(point, Vector3.zero); hasBounds = true; }
                else bounds.Encapsulate(point);
            }
        }
        var box = GetComponent<BoxCollider>();
        box.isTrigger = false;
        if (hasBounds) { box.center = bounds.center; box.size = bounds.size; }
    }

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
        SetInteractionEnabled(RemainingGold > 0 && !isMiningThisOre);
    }

    public override void InteractA()
    {
        if (this.GetSystem<IGamePauseSystem>().IsPaused) return;

        var role = this.GetSystem<IRoleInstanceSystem>().ControllingRole;
        var player = role != null ? role.PlayerStateMachine : null;
        if (player == null) return;
        var context = new MineContext(mineId, transform.position);
        if (!miningSystem.IsAvailable(context)) return;

        if (player.StateMachine.IsInState<PlayerMineState>()) return;

        // 状态机先处理退出时，同一帧的交互输入不能再次进入挖矿。
        if (player.StateMachine.PreviousState is PlayerMineState previousMine
            && previousMine.LastExitFrame == Time.frameCount) return;

        player.ChangeState<PlayerMineState>(context);
        UpdateInteractionAvailability();
    }
}
