using System.Collections.Generic;
using QFramework;
using Unity.Cinemachine;
using UnityEngine;

public interface ICinemaChineCameraSystem : ISystem
{
    void RegisterCinemaChineCamera(string key, CinemachineCamera camera);
    void UnregisterCinemaChineCamera(string key);
    void SetCinemaChineCamera(string key);
    void TransitionTo(string key, float speed = 1f);
}

public class CinemaChineCameraSystem : AbstractSystem, ICinemaChineCameraSystem
{
    private readonly Dictionary<string, CinemachineCamera> _cameras = new();

    private CinemachineBrain _brain;

    private const int HighPriority = 100;
    private const int LowPriority = 0;

    protected override void OnInit() { }

    protected override void OnDeinit()
    {
        _cameras.Clear();
        _brain = null;
    }

    public void RegisterCinemaChineCamera(string key, CinemachineCamera camera)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError($"{nameof(CinemaChineCameraSystem)}: 注册相机失败，key为空");
            return;
        }

        if (camera == null)
        {
            Debug.LogError($"{nameof(CinemaChineCameraSystem)}: 注册相机失败，camera为null (key: {key})");
            return;
        }

        _cameras[key] = camera;
        camera.Priority = new PrioritySettings { Enabled = true };
        camera.Priority.Value = LowPriority;
    }

    public void UnregisterCinemaChineCamera(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError($"{nameof(CinemaChineCameraSystem)}: 注销相机失败，key为空");
            return;
        }

        if (!_cameras.ContainsKey(key))
        {
            Debug.LogWarning($"{nameof(CinemaChineCameraSystem)}: 注销相机失败，不存在key: {key}");
        }

        _cameras.Remove(key);
    }

    public void SetCinemaChineCamera(string key)
    {
        if (!TryGetCamera(key, out var targetCamera)) return;

        EnsureBrain();

        if (_brain != null)
        {
            var originalBlend = _brain.DefaultBlend;
            _brain.DefaultBlend = new CinemachineBlendDefinition
            {
                Style = CinemachineBlendDefinition.Styles.Cut,
                Time = 0f
            };

            ActivateCamera(targetCamera);

            _brain.DefaultBlend = originalBlend;
        }
        else
        {
            ActivateCamera(targetCamera);
        }
    }

    public void TransitionTo(string key, float speed = 1f)
    {
        if (!TryGetCamera(key, out var targetCamera)) return;

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

        ActivateCamera(targetCamera);
    }

    private void ActivateCamera(CinemachineCamera targetCamera)
    {
        foreach (var kvp in _cameras)
        {
            kvp.Value.Priority = new PrioritySettings { Enabled = true };
            kvp.Value.Priority.Value = LowPriority;
        }

        targetCamera.Priority = new PrioritySettings { Enabled = true };
        targetCamera.Priority.Value = HighPriority;
        targetCamera.Prioritize();
    }

    private bool TryGetCamera(string key, out CinemachineCamera camera)
    {
        camera = null;
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError($"{nameof(CinemaChineCameraSystem)}: key为空");
            return false;
        }

        if (!_cameras.TryGetValue(key, out camera))
        {
            Debug.LogError($"{nameof(CinemaChineCameraSystem)}: 不存在key: {key}");
            return false;
        }

        if (camera == null)
        {
            Debug.LogError($"{nameof(CinemaChineCameraSystem)}: 相机已被销毁 (key: {key})");
            _cameras.Remove(key);
            return false;
        }

        return true;
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
