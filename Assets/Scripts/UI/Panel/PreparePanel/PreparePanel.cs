using QFramework;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PreparePanel : UIPanel
{
    [SerializeField] private Button _letSGoBtn;
    [SerializeField] private Button _studyBtn;
    [SerializeField] private Button _workBtn;

    public event UnityAction onLetSGoBtnClicked;
    public event UnityAction onStudyBtnClicked;
    public event UnityAction onWorkBtnClicked;

    public event UnityAction onLetSGoBtnHoverEnter;
    public event UnityAction onLetSGoBtnHoverExit;
    public event UnityAction onStudyBtnHoverEnter;
    public event UnityAction onStudyBtnHoverExit;
    public event UnityAction onWorkBtnHoverEnter;
    public event UnityAction onWorkBtnHoverExit;

    protected override void OnInit(IUIData uiData = null)
    {
        _letSGoBtn.onClick.AddListener(() => onLetSGoBtnClicked?.Invoke());
        _studyBtn.onClick.AddListener(() => onStudyBtnClicked?.Invoke());
        _workBtn.onClick.AddListener(() => onWorkBtnClicked?.Invoke());

        AddHoverEvents(_letSGoBtn, onLetSGoBtnHoverEnter, onLetSGoBtnHoverExit);
        AddHoverEvents(_studyBtn, onStudyBtnHoverEnter, onStudyBtnHoverExit);
        AddHoverEvents(_workBtn, onWorkBtnHoverEnter, onWorkBtnHoverExit);
    }

    private void AddHoverEvents(Button button, UnityAction onEnter, UnityAction onExit)
    {
        var trigger = button.gameObject.GetComponent<EventTrigger>();
        if (!trigger) trigger = button.gameObject.AddComponent<EventTrigger>();

        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener(_ => onEnter?.Invoke());
        trigger.triggers.Add(enterEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener(_ => onExit?.Invoke());
        trigger.triggers.Add(exitEntry);
    }

    protected override void OnClose()
    {
        _letSGoBtn.onClick.RemoveAllListeners();
        _studyBtn.onClick.RemoveAllListeners();
        _workBtn.onClick.RemoveAllListeners();
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
