using QFramework;
using UnityEngine;

public class StartController : MonoBehaviour, IController
{
    [SerializeField] private StartPanel _panel;

    void Awake()
    {
        _panel = GetComponent<StartPanel>();

        // 订阅事件
        _panel.onStartBtnClicked += OnStartBtnClicked;
        _panel.onSettingBtnClicked += OnSettingBtnClicked;
        _panel.onAboutBtnClicked += OnAboutBtnClicked;
        _panel.onExitBtnClicked += OnExitBtnClicked;
    }

    void OnDestroy()
    {
        // 取消订阅
        if (_panel != null)
        {
            _panel.onStartBtnClicked -= OnStartBtnClicked;
            _panel.onSettingBtnClicked -= OnSettingBtnClicked;
            _panel.onAboutBtnClicked -= OnAboutBtnClicked;
            _panel.onExitBtnClicked -= OnExitBtnClicked;
        }
    }

    private void OnStartBtnClicked()
    {
        Debug.Log("开始游戏");
        // TODO: 实现开始游戏逻辑
    }

    private void OnSettingBtnClicked()
    {
        Debug.Log("打开设置");
        // TODO: 实现打开设置逻辑
    }

    private void OnAboutBtnClicked()
    {
        Debug.Log("关于游戏");
        // TODO: 实现关于游戏逻辑
    }

    private void OnExitBtnClicked()
    {
        Debug.Log("退出游戏");
        // TODO: 实现退出游戏逻辑
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

	public IArchitecture GetArchitecture()
	{
		return TianArchitecture.Interface;
	}
}
