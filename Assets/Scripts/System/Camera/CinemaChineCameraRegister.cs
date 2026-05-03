using QFramework;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CinemaChineCameraRegister : MonoBehaviour, ICanGetSystem
{
    [SerializeField] private string key;

    private void Awake()
    {
        if (string.IsNullOrEmpty(key)) return;

        var camera = GetComponent<CinemachineCamera>();
        this.GetSystem<ICinemaChineCameraSystem>().RegisterCinemaChineCamera(key, camera);
    }

    private void OnDestroy()
    {
        if (string.IsNullOrEmpty(key)) return;

        this.GetSystem<ICinemaChineCameraSystem>()?.UnregisterCinemaChineCamera(key);
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
