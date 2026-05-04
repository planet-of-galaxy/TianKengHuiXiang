using QFramework;
using UnityEngine;

[RequireComponent(typeof(PreparePanel))]
public class PrepareController : MonoBehaviour, IController
{
    private PreparePanel _panel;

    // 虚拟相机切换速度
    private float _transCamSpeed = 2f;

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
        this.GetSystem<IPlayerSystem>().InitPlayer(0);
        this.SendCommand<ChangeProcedureStateCmd<MineCaveState>>();
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
        this.SendCommand(new TransitionCameraCmd("LetSGo", _transCamSpeed));
    }

    private void OnLetSGoBtnHoverExit() { }

    private void OnStudyBtnHoverEnter()
    {
        this.SendCommand(new TransitionCameraCmd("Study", _transCamSpeed));
    }

    private void OnStudyBtnHoverExit() { }

    private void OnWorkBtnHoverEnter()
    {
        this.SendCommand(new TransitionCameraCmd("Work", _transCamSpeed));
    }

    private void OnWorkBtnHoverExit() { }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
