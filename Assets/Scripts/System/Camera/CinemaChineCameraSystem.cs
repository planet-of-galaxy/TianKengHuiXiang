using QFramework;
using Unity.Cinemachine;
using UnityEngine;

public interface ICinemaChineCameraSystem : ISystem
{
    void SetCinemaChineCamera(CinemachineCamera camera);
    void TransitionTo(CinemachineCamera camera, float speed = 1f);
    CinemachineCamera GetCurrentCinema();
}

public class CinemaChineCameraSystem : AbstractSystem, ICinemaChineCameraSystem
{
    private CinemachineBrain _brain;
    private CinemachineCamera _currentCamera;

    private const int HighPriority = 100;
    private const int LowPriority = 0;

    protected override void OnInit() { }

    protected override void OnDeinit()
    {
        _currentCamera = null;
        _brain = null;
    }

    public void SetCinemaChineCamera(CinemachineCamera camera)
    {
        if (camera == null)
        {
            Debug.LogError($"{nameof(CinemaChineCameraSystem)}: 设置相机失败，camera为null");
            return;
        }

        EnsureBrain();

        if (_brain != null)
        {
            var originalBlend = _brain.DefaultBlend;
            _brain.DefaultBlend = new CinemachineBlendDefinition
            {
                Style = CinemachineBlendDefinition.Styles.Cut,
                Time = 0f
            };

            ActivateCamera(camera);

            _brain.DefaultBlend = originalBlend;
        }
        else
        {
            ActivateCamera(camera);
        }
    }

    public void TransitionTo(CinemachineCamera camera, float speed = 1f)
    {
        if (camera == null)
        {
            Debug.LogError($"{nameof(CinemaChineCameraSystem)}: 过渡相机失败，camera为null");
            return;
        }

        EnsureBrain();

        if (_brain != null)
        {
            float blendTime = Mathf.Max(0.01f, 1f / speed);
            _brain.DefaultBlend = new CinemachineBlendDefinition
            {
                Style = CinemachineBlendDefinition.Styles.EaseInOut,
                Time = blendTime
            };
        }

        ActivateCamera(camera);
    }

    public CinemachineCamera GetCurrentCinema()
    {
        return _currentCamera;
    }

    private void ActivateCamera(CinemachineCamera targetCamera)
    {
        if (_currentCamera != null)
        {
            _currentCamera.Priority = new PrioritySettings { Enabled = true };
            _currentCamera.Priority.Value = LowPriority;
        }

        targetCamera.Priority = new PrioritySettings { Enabled = true };
        targetCamera.Priority.Value = HighPriority;
        targetCamera.Prioritize();

        _currentCamera = targetCamera;
    }

    private void EnsureBrain()
    {
        if (_brain != null) return;

        var mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError($"{nameof(CinemaChineCameraSystem)}: 未找到主相机 (Camera.main)");
            return;
        }

        _brain = mainCam.GetComponent<CinemachineBrain>();
        if (_brain == null)
        {
            _brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
        }
    }
}
