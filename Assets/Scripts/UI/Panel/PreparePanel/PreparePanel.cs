using QFramework;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

public class PreparePanel : UIPanel
{
    [SerializeField] private Button _letSGoBtn;
    [SerializeField] private Button _studyBtn;
    [SerializeField] private Button _workBtn;

    public event UnityAction onLetSGoBtnClicked;
    public event UnityAction onStudyBtnClicked;
    public event UnityAction onWorkBtnClicked;

    protected override void OnInit(IUIData uiData = null)
    {
        _letSGoBtn.onClick.AddListener(() => onLetSGoBtnClicked?.Invoke());
        _studyBtn.onClick.AddListener(() => onStudyBtnClicked?.Invoke());
        _workBtn.onClick.AddListener(() => onWorkBtnClicked?.Invoke());
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
