using QFramework;
using UnityEngine;

public interface ICameraSystem : ISystem
{
    /// <summary>
    /// 立即将主相机设为目标参数
    /// </summary>
    void SetCamera(CameraInfo info);

    /// <summary>
    /// 平滑过渡主相机到目标参数
    /// </summary>
    /// <param name="info">目标相机参数</param>
    /// <param name="speed">过渡速度，值越大越快，默认为1</param>
    void TransitionTo(CameraInfo info, float speed = 1f);
}

public class CameraSystem : AbstractSystem, ICameraSystem
{
    private CameraSystemUpdater _updater;

    private bool _isTransitioning;
    private CameraInfo _targetInfo;
    private float _transitionSpeed;

    private const float PositionThreshold = 0.001f;
    private const float RotationThreshold = 0.01f;
    private const float FOVThreshold = 0.01f;

    protected override void OnInit()
    {
        var go = new GameObject("[CameraSystemUpdater]");
        Object.DontDestroyOnLoad(go);
        _updater = go.AddComponent<CameraSystemUpdater>();
        _updater.Init(this);
    }

    protected override void OnDeinit()
    {
        if (_updater != null)
        {
            Object.Destroy(_updater.gameObject);
            _updater = null;
        }
    }

    public void SetCamera(CameraInfo info)
    {
        _isTransitioning = false;

        var cam = Camera.main;
        if (cam == null) return;

        cam.transform.position = info.Position;
        cam.transform.rotation = info.Rotation;
        cam.fieldOfView = info.FOV;
    }

    public void TransitionTo(CameraInfo info, float speed = 1f)
    {
        _targetInfo = info;
        _transitionSpeed = speed;
        _isTransitioning = true;
    }

    internal void Update(float deltaTime)
    {
        if (!_isTransitioning) return;

        var cam = Camera.main;
        if (cam == null) return;

        float t = _transitionSpeed * deltaTime;

        cam.transform.position = Vector3.Lerp(cam.transform.position, _targetInfo.Position, t);
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, _targetInfo.Rotation, t);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, _targetInfo.FOV, t);

        // 足够接近时 snap 到目标，结束过渡
        bool posReached = Vector3.Distance(cam.transform.position, _targetInfo.Position) < PositionThreshold;
        bool rotReached = Quaternion.Angle(cam.transform.rotation, _targetInfo.Rotation) < RotationThreshold;
        bool fovReached = Mathf.Abs(cam.fieldOfView - _targetInfo.FOV) < FOVThreshold;

        if (posReached && rotReached && fovReached)
        {
            cam.transform.position = _targetInfo.Position;
            cam.transform.rotation = _targetInfo.Rotation;
            cam.fieldOfView = _targetInfo.FOV;
            _isTransitioning = false;
        }
    }
}

public class CameraSystemUpdater : MonoBehaviour
{
    private CameraSystem _system;

    public void Init(CameraSystem system)
    {
        _system = system;
    }

    private void Update()
    {
        _system?.Update(Time.deltaTime);
    }
}
