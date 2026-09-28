using System;
using QFramework;

public interface IQuestSystem : ISystem
{
    T CreateQuest<T>() where T : IQuest, new();
    void CloseQuest(IQuest quest);
}

public class QuestSystem : AbstractSystem, IQuestSystem
{
    private IQuestModel questModel;

    protected override void OnInit()
    {
        questModel = this.GetModel<IQuestModel>();
    }

    public T CreateQuest<T>() where T : IQuest, new()
    {
        var quest = new T();
        questModel.AddQuest(quest);
        return quest;
    }

    /// <summary>先从模型移除，再触发关闭事件，事件订阅者可读取更新后的任务集合。</summary>
    public void CloseQuest(IQuest quest)
    {
        if (quest == null) throw new ArgumentNullException(nameof(quest));
        var belongsToModel = false;
        foreach (var item in questModel.QuestList)
        {
            if (ReferenceEquals(item, quest))
            {
                belongsToModel = true;
                break;
            }
        }

        if (!belongsToModel) return;
        questModel.RemoveQuest(quest);
        quest.Close();
    }
}
