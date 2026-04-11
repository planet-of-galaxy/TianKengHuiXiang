using QFramework;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

public class StartPanel : UIPanel, IController
{
    [SerializeField] private Button _startBtn;
    [SerializeField] private Button _settingBtn;
    [SerializeField] private Button _aboutBtn;
    [SerializeField] private Button _exitBtn;

    public event UnityAction onStartBtnClicked;
    public event UnityAction onSettingBtnClicked;
    public event UnityAction onAboutBtnClicked;
    public event UnityAction onExitBtnClicked;

    protected override void OnInit(IUIData uiData = null)
    {
        _startBtn.onClick.AddListener(() => onStartBtnClicked?.Invoke());
        _settingBtn.onClick.AddListener(() => onSettingBtnClicked?.Invoke());
        _aboutBtn.onClick.AddListener(() => onAboutBtnClicked?.Invoke());
        _exitBtn.onClick.AddListener(() => onExitBtnClicked?.Invoke());
    }

    protected override void OnClose()
    {
        _startBtn.onClick.RemoveAllListeners();
        _settingBtn.onClick.RemoveAllListeners();
        _aboutBtn.onClick.RemoveAllListeners();
        _exitBtn.onClick.RemoveAllListeners();
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
