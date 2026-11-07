using UnityEngine;

/// <summary>本次挖矿的目标 ID 和世界坐标快照。</summary>
public sealed class MineContext
{
    public int MineId { get; }
    public Vector3 Position { get; }

    public MineContext(int mineId, Vector3 position)
    {
        MineId = mineId;
        Position = position;
    }
}
