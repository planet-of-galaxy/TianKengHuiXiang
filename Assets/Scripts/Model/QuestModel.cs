using System;
using System.Collections.Generic;
using QFramework;

public interface IQuestModel : IModel
{
    int QuestCount { get; }
    IReadOnlyList<IQuest> QuestList { get; }
    void AddQuest(IQuest quest);
    void RemoveQuest(IQuest quest);
}

public class QuestModel : AbstractModel, IQuestModel
{
    private readonly List<IQuest> quests = new();
    private readonly IReadOnlyList<IQuest> questList;

    public QuestModel()
    {
        questList = quests.AsReadOnly();
    }

    public int QuestCount => quests.Count;
    public IReadOnlyList<IQuest> QuestList => questList;

    public void AddQuest(IQuest quest)
    {
        if (quest == null) throw new ArgumentNullException(nameof(quest));
        if (quest.State == QuestState.Closed)
            throw new InvalidOperationException("已关闭的任务不能重新添加。");

        if (quests.Contains(quest)) return;
        quests.Add(quest);
        quest.OnQuestClosed += RemoveClosedQuests;
    }

    public void RemoveQuest(IQuest quest)
    {
        if (quest == null) throw new ArgumentNullException(nameof(quest));
        if (quests.Remove(quest)) quest.OnQuestClosed -= RemoveClosedQuests;
    }

    private void RemoveClosedQuests()
    {
        for (var i = quests.Count - 1; i >= 0; i--)
        {
            if (quests[i].State == QuestState.Closed) RemoveQuest(quests[i]);
        }
    }

    protected override void OnInit() { }
}
