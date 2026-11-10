using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface IMiningSystem : ISystem
{
    int Register(MiningController mine);
    void Unregister(MiningController mine);
    bool IsAvailable(MineContext context);
    int GetRemainingGold(int mineId);
    /// <summary>当前单块金矿的采集进度，范围为 0～1。</summary>
    float GetMiningProgress(RoleContext miner);
    void BeginMining(RoleContext miner);
    void EndMining(RoleContext miner);
    bool TickMining(RoleContext miner, MineContext context, float deltaTime);
}

/// <summary>管理金矿登记、逐秒结算与耗尽销毁。</summary>
public class MiningSystem : AbstractSystem, IMiningSystem
{
    /// <summary>采集 1 个金矿所需的持续挖矿时间（秒）。</summary>
    private const float SecondsPerGold = 10f;

    /// <summary>金矿道具在 ItemConfig 中的 itemId。</summary>
    private const int GoldOreItemId = 0;

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

    public float GetMiningProgress(RoleContext miner)
    {
        return miner != null && elapsedTimes.TryGetValue(miner, out float elapsed)
            ? Mathf.Clamp01(elapsed / SecondsPerGold) : 0f;
    }

    /// <returns>目标和挖矿者仍有效、金矿尚未耗尽且背包仍可接收金矿。</returns>
    public bool TickMining(RoleContext miner, MineContext context, float deltaTime)
    {
        if (miner == null || !IsAvailable(context)) return false;
        if (this.GetSystem<IGamePauseSystem>().IsPaused) return true;

        elapsedTimes.TryGetValue(miner, out float elapsed);
        elapsed += Mathf.Max(0f, deltaTime);
        var mine = mines[context.MineId];
        var packageSystem = this.GetSystem<IPackageSystem>();
        // 无法接收金矿时立即结束挖矿，不等待下一次结算，也不扣除矿脉数量。
        if (!packageSystem.HasSpaceForItem(miner.RoleRuntimeIndex, ItemType.Item, GoldOreItemId))
        {
            TipUtility.ShowTip("背包已满");
            EndMining(miner);
            return false;
        }

        // 每 SecondsPerGold 秒采集 1 个金矿，只有成功入包后才扣除矿脉数量。
        while (elapsed >= SecondsPerGold && mine.RemainingGold > 0)
        {
            if (!packageSystem.AddItemToRolePackage(miner.RoleRuntimeIndex, ItemType.Item, GoldOreItemId))
            {
                EndMining(miner);
                return false;
            }
            elapsed -= SecondsPerGold;
            mine.RemainingGold -= 1;
            // 本块入包后立即检查下一块的容量，装满时不再开始新的采集。
            if (mine.RemainingGold > 0
                && !packageSystem.HasSpaceForItem(miner.RoleRuntimeIndex, ItemType.Item, GoldOreItemId))
            {
                TipUtility.ShowTip("背包已满");
                EndMining(miner);
                return false;
            }
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
