using QFramework;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CinemaChineCameraRegister : MonoBehaviour, ICanGetSystem
{
    [SerializeField] private string key;

    private void Awake()
    {
        Debug.Log($"[CinemaChineCameraRegister] Awake called for key: '{key}'");

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning($"[CinemaChineCameraRegister] Key is empty on {gameObject.name}");
            return;
        }

        var camera = GetComponent<CinemachineCamera>();
        if (camera == null)
        {
            Debug.LogError($"[CinemaChineCameraRegister] No CinemachineCamera found on {gameObject.name}");
            return;
        }

        Debug.Log($"[CinemaChineCameraRegister] Registering camera with key: {key}");
        this.GetSystem<ICinemaChineCameraSystem>().RegisterCinemaChineCamera(key, camera);
    }

    private void OnDestroy()
    {
        if (string.IsNullOrEmpty(key)) return;

        Debug.Log($"[CinemaChineCameraRegister] UnRegistering camera with key: {key}");
        this.GetSystem<ICinemaChineCameraSystem>()?.UnregisterCinemaChineCamera(key);
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
