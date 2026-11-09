using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface IMiningSystem : ISystem
{
    int Register(MiningController mine);
    void Unregister(MiningController mine);
    bool IsAvailable(MineContext context);
    int GetRemainingGold(int mineId);
    void BeginMining(RoleContext miner);
    void EndMining(RoleContext miner);
    bool TickMining(RoleContext miner, MineContext context, float deltaTime);
}

/// <summary>管理金矿登记、逐秒结算与耗尽销毁。</summary>
public class MiningSystem : AbstractSystem, IMiningSystem
{
    /// <summary>采集 1 个黄金所需的持续挖矿时间（秒）。</summary>
    private const float SecondsPerGold = 10f;

    private readonly Dictionary<int, MiningController> mines = new();
    private readonly Dictionary<RoleContext, float> elapsedTimes = new();
    private int nextId = 1;

    protected override void OnInit() { }

    protected override void OnDeinit()
    {
        mines.Clear();
        elapsedTimes.Clear();
    }

    public int Register(MiningController mine)
    {
        int id = mine.MineId;
        // 同一预制体的多份实例也必须有独立的运行时 ID。
        if (id <= 0 || (mines.TryGetValue(id, out var existing) && existing != mine))
        {
            while (mines.ContainsKey(nextId)) nextId++;
            id = nextId++;
        }
        mines[id] = mine;
        return id;
    }

    public void Unregister(MiningController mine)
    {
        if (mines.TryGetValue(mine.MineId, out var registered) && registered == mine)
            mines.Remove(mine.MineId);
    }

    public bool IsAvailable(MineContext context)
    {
        return context != null && mines.TryGetValue(context.MineId, out var mine)
            && mine != null && mine.isActiveAndEnabled && mine.RemainingGold > 0
            && mine.transform.position == context.Position;
    }

    public int GetRemainingGold(int mineId)
    {
        return mines.TryGetValue(mineId, out var mine) && mine != null ? mine.RemainingGold : 0;
    }

    public void BeginMining(RoleContext miner) => elapsedTimes[miner] = 0f;
    public void EndMining(RoleContext miner) => elapsedTimes.Remove(miner);

    /// <returns>目标和挖矿者仍有效且金矿尚未耗尽。</returns>
    public bool TickMining(RoleContext miner, MineContext context, float deltaTime)
    {
        if (miner == null || !IsAvailable(context)
            || !this.GetModel<IRoleRuntimeModel>().TryGetRoleRuntime(miner.RoleRuntimeIndex, out var runtime))
            return false;
        if (this.GetSystem<IGamePauseSystem>().IsPaused) return true;

        elapsedTimes.TryGetValue(miner, out float elapsed);
        elapsed += Mathf.Max(0f, deltaTime);
        var mine = mines[context.MineId];
        // 每 SecondsPerGold 秒产出 1 个黄金；一帧跨过多个周期时补齐结算，最后一笔不会超过剩余储量。
        while (elapsed >= SecondsPerGold && mine.RemainingGold > 0)
        {
            elapsed -= SecondsPerGold;
            int amount = Mathf.Min(1, mine.RemainingGold);
            mine.RemainingGold -= amount;
            runtime.Gold.Value += amount;
        }
        elapsedTimes[miner] = elapsed;
        if (mine.RemainingGold > 0) return true;

        Unregister(mine);
        mine.PrepareForRemoval();
        Object.Destroy(mine.gameObject);
        elapsedTimes.Remove(miner);
        return false;
    }
}
