using System;

public enum QuestState
{
    Running,
    Completed,
    Failed,
    Closed
}

/// <summary>通用失败原因，可按具体任务需求扩展。</summary>
public enum QuestFailedCode
{
    Unknown,
    Timeout,
    ConditionNotMet
}

public interface IQuest
{
    QuestState State { get; }
    event Action OnQuestCompleted;
    event Action<QuestFailedCode> OnQuestFailed;
    event Action OnQuestClosed;

    /// <summary>关闭并释放任务资源；应保证重复调用不会重复通知。</summary>
    void Close();
}

/// <summary>
/// 任务创建后即处于运行状态。具体任务通过 Complete/Fail 结束执行，
/// 完成或失败后仍保留在模型中，直到调用 IQuestSystem.CloseQuest。
/// </summary>
public abstract class QuestBase : IQuest
{
    public QuestState State { get; private set; } = QuestState.Running;
    public event Action OnQuestCompleted;
    public event Action<QuestFailedCode> OnQuestFailed;
    public event Action OnQuestClosed;

    protected void Complete()
    {
        if (State != QuestState.Running) return;
        State = QuestState.Completed;
        OnQuestCompleted?.Invoke();
    }

    protected void Fail(QuestFailedCode code)
    {
        if (State != QuestState.Running) return;
        State = QuestState.Failed;
        OnQuestFailed?.Invoke(code);
    }

    public void Close()
    {
        if (State == QuestState.Closed) return;
        State = QuestState.Closed;
        try
        {
            OnClose();
        }
        finally
        {
            try
            {
                OnQuestClosed?.Invoke();
            }
            finally
            {
                OnQuestCompleted = null;
                OnQuestFailed = null;
                OnQuestClosed = null;
            }
        }
    }

    /// <summary>在此取消外部事件订阅、计时器等任务资源。</summary>
    protected virtual void OnClose() { }
}
