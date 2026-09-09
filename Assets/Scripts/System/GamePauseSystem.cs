using QFramework;
using UnityEngine;

/// <summary>
/// 全局游戏暂停系统接口：暂停/恢复游戏并暴露暂停状态。
/// 打开需要独占操作 UI 的界面（背包/仓库等）时调用 <see cref="Pause"/>，
/// 关闭界面时调用 <see cref="Resume"/>，取代各处直接调用 CursorUtility 的分散写法。
/// </summary>
public interface IGamePauseSystem : ISystem
{
    /// <summary>当前是否处于暂停状态。</summary>
    bool IsPaused { get; }

    /// <summary>进入暂停：Time.timeScale 置 0，并显示、解锁鼠标。</summary>
    void Pause();

    /// <summary>退出一次暂停。支持多次 Pause（多个来源），需全部 Resume 后才会真正恢复。</summary>
    void Resume();
}

/// <summary>
/// 全局暂停系统（QFramework System，由 TianArchitecture 注册）。
/// 集中处理暂停的两个副作用：时间缩放与鼠标光标。
/// 首次 Pause 时记录进入前的 timeScale 与光标状态，仅在全部暂停来源退出后还原，
/// 保证“从哪里暂停、恢复回哪里”。
/// </summary>
public class GamePauseSystem : AbstractSystem, IGamePauseSystem
{
    /// <summary>暂停请求计数：大于 0 即视为暂停。背包/仓库等面板各自 Pause/Resume，全部恢复才真正恢复。</summary>
    private int pauseRequestCount;

    /// <summary>暂停前的时间缩放（首次 Pause 时记录），全部恢复时还原。</summary>
    private float timeScaleBeforePause = 1f;

    /// <summary>暂停前的光标状态快照（首次 Pause 时记录），全部恢复时还原。</summary>
    private CursorUtility.CursorSnapshot cursorBeforePause;

    public bool IsPaused => pauseRequestCount > 0;

    protected override void OnInit()
    {
    }

    public void Pause()
    {
        if (pauseRequestCount == 0)
        {
            // 首次进入暂停：记录进入前的状态，供退出时还原
            timeScaleBeforePause = Time.timeScale;
            cursorBeforePause = CursorUtility.Capture();
        }

        pauseRequestCount++;

        // 暂停副作用：时间静止 + 显示并解锁鼠标（供操作 UI）
        Time.timeScale = 0f;
        CursorUtility.ShowAndUnlock();
    }

    public void Resume()
    {
        if (pauseRequestCount <= 0)
        {
            return; // 未处于暂停，忽略多余的恢复
        }

        pauseRequestCount--;
        if (pauseRequestCount > 0)
        {
            return; // 仍有其它暂停来源，保持暂停
        }

        // 全部暂停来源已恢复：还原暂停前的 timeScale 与光标状态
        Time.timeScale = timeScaleBeforePause;
        cursorBeforePause.Restore();
    }
}
