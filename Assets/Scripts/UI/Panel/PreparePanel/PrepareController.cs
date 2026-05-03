using QFramework;
using UnityEngine;

[RequireComponent(typeof(PreparePanel))]
public class PrepareController : MonoBehaviour, IController
{
    private PreparePanel _panel;

    void Awake()
    {
        _panel = GetComponent<PreparePanel>();

        _panel.onLetSGoBtnClicked += OnLetSGoBtnClicked;
        _panel.onStudyBtnClicked += OnStudyBtnClicked;
        _panel.onWorkBtnClicked += OnWorkBtnClicked;

        _panel.onLetSGoBtnHoverEnter += OnLetSGoBtnHoverEnter;
        _panel.onLetSGoBtnHoverExit += OnLetSGoBtnHoverExit;
        _panel.onStudyBtnHoverEnter += OnStudyBtnHoverEnter;
        _panel.onStudyBtnHoverExit += OnStudyBtnHoverExit;
        _panel.onWorkBtnHoverEnter += OnWorkBtnHoverEnter;
        _panel.onWorkBtnHoverExit += OnWorkBtnHoverExit;
    }

    void OnDestroy()
    {
        if (_panel != null)
        {
            _panel.onLetSGoBtnClicked -= OnLetSGoBtnClicked;
            _panel.onStudyBtnClicked -= OnStudyBtnClicked;
            _panel.onWorkBtnClicked -= OnWorkBtnClicked;

            _panel.onLetSGoBtnHoverEnter -= OnLetSGoBtnHoverEnter;
            _panel.onLetSGoBtnHoverExit -= OnLetSGoBtnHoverExit;
            _panel.onStudyBtnHoverEnter -= OnStudyBtnHoverEnter;
            _panel.onStudyBtnHoverExit -= OnStudyBtnHoverExit;
            _panel.onWorkBtnHoverEnter -= OnWorkBtnHoverEnter;
            _panel.onWorkBtnHoverExit -= OnWorkBtnHoverExit;
        }
    }

    private void OnLetSGoBtnClicked()
    {
        Debug.Log("出发！");
        // TODO: 实现出发逻辑
    }

    private void OnStudyBtnClicked()
    {
        Debug.Log("学习");
        // TODO: 实现学习逻辑
    }

    private void OnWorkBtnClicked()
    {
        Debug.Log("工作");
        // TODO: 实现工作逻辑
    }

    private void OnLetSGoBtnHoverEnter()
    {
        this.SendCommand(new TransitionCameraCmd("LetSGo"));
    }

    private void OnLetSGoBtnHoverExit()
    {
        this.SendCommand(new TransitionCameraCmd("LetSGo"));
    }

    private void OnStudyBtnHoverEnter()
    {
        this.SendCommand(new TransitionCameraCmd("Study"));
    }

    private void OnStudyBtnHoverExit()
    {
        this.SendCommand(new TransitionCameraCmd("LetSGo"));
    }

    private void OnWorkBtnHoverEnter()
    {
        this.SendCommand(new TransitionCameraCmd("Work"));
    }

    private void OnWorkBtnHoverExit()
    {
        this.SendCommand(new TransitionCameraCmd("LetSGo"));
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
