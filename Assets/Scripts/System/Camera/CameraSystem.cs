using QFramework;
using UnityEngine;

/// <summary>
/// 相机系统：负责主相机的生成与状态管理，并提供 UI 相机显隐控制接口。
/// 主相机从 Resources/Prefabe/Camera/Main Camera 预制体实例化，根节点为 Main Camera，
/// 子节点 UI Camera 负责 UI 层渲染（CameraType=UI，仅渲染 UI 层）。
/// </summary>
public interface ICameraSystem : ISystem
{
    /// <summary>主相机（预制体根节点）</summary>
    Camera MainCamera { get; }

    /// <summary>UI 相机（预制体子节点）</summary>
    Camera UICamera { get; }

    /// <summary>是否已生成相机</summary>
    bool IsCreated { get; }

    /// <summary>UI 相机当前是否可见</summary>
    bool IsUICameraVisible { get; }

    /// <summary>
    /// 从预制体生成相机；已生成则忽略。首次生成时会清理场景中已存在的主相机，保证唯一。
    /// </summary>
    void CreateCamera();

    /// <summary>设置 UI 相机显隐</summary>
    void SetUICameraVisible(bool visible);

    /// <summary>显示 UI 相机</summary>
    void ShowUICamera();

    /// <summary>隐藏 UI 相机</summary>
    void HideUICamera();
}

public class CameraSystem : AbstractSystem, ICameraSystem
{
    private const string CameraPrefabPath = "Prefabe/Camera/Main Camera";
    private const string MainCameraTag = "MainCamera";
    private const string UICameraName = "UI Camera";

    private IResourceStorage _resourceStorage;
    private GameObject _cameraRoot;
    private Camera _mainCamera;
    private Camera _uiCamera;

    public Camera MainCamera => _mainCamera;
    public Camera UICamera => _uiCamera;
    public bool IsCreated => _cameraRoot != null;
    public bool IsUICameraVisible => _uiCamera != null && _uiCamera.enabled;

    protected override void OnInit()
    {
        _resourceStorage = this.GetUtility<IResourceStorage>();
        CreateCamera();
    }

    protected override void OnDeinit()
    {
        if (_cameraRoot != null)
        {
            Object.Destroy(_cameraRoot);
            _cameraRoot = null;
        }
        _mainCamera = null;
        _uiCamera = null;
    }

    public void CreateCamera()
    {
        if (IsCreated)
        {
            Debug.Log($"[{nameof(CameraSystem)}] 相机已生成，忽略重复创建");
            return;
        }

        // 清理场景中已放置的主相机，避免生成后出现双相机
        var existingMain = GameObject.FindGameObjectWithTag(MainCameraTag);
        if (existingMain != null)
        {
            Object.Destroy(existingMain);
        }

        var prefab = _resourceStorage.Load<GameObject>(CameraPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[{nameof(CameraSystem)}] 预制体加载失败: {CameraPrefabPath}");
            return;
        }

        _cameraRoot = Object.Instantiate(prefab);
        Object.DontDestroyOnLoad(_cameraRoot);

        _mainCamera = _cameraRoot.GetComponent<Camera>();
        if (_mainCamera == null)
        {
            Debug.LogError($"[{nameof(CameraSystem)}] 预制体根节点缺少 Camera 组件: {CameraPrefabPath}");
        }

        var uiTransform = _cameraRoot.transform.Find(UICameraName);
        if (uiTransform != null)
        {
            _uiCamera = uiTransform.GetComponent<Camera>();
        }
        if (_uiCamera == null)
        {
            Debug.LogError($"[{nameof(CameraSystem)}] 预制体缺少子节点 {UICameraName}: {CameraPrefabPath}");
        }

        Debug.Log($"[{nameof(CameraSystem)}] 相机生成完成: {_cameraRoot.name}");
    }

    public void SetUICameraVisible(bool visible)
    {
        if (_uiCamera == null)
        {
            Debug.LogWarning($"[{nameof(CameraSystem)}] UI 相机未生成，无法设置显隐");
            return;
        }

        _uiCamera.enabled = visible;
        Debug.Log($"[{nameof(CameraSystem)}] UI 相机 {(visible ? "显示" : "隐藏")}");
    }

    public void ShowUICamera()
    {
        SetUICameraVisible(true);
    }

    public void HideUICamera()
    {
        SetUICameraVisible(false);
    }
}
