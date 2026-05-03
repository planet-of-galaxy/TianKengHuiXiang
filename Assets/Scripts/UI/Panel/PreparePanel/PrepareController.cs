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
    }

    void OnDestroy()
    {
        if (_panel != null)
        {
            _panel.onLetSGoBtnClicked -= OnLetSGoBtnClicked;
            _panel.onStudyBtnClicked -= OnStudyBtnClicked;
            _panel.onWorkBtnClicked -= OnWorkBtnClicked;
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

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
