using QFramework;
using Unity.Cinemachine;
using UnityEngine;

public class TransitionCameraCmd : AbstractCommand
{
    private readonly CinemachineCamera _camera;
    private readonly float _speed;

    public TransitionCameraCmd(CinemachineCamera camera, float speed = 1f)
    {
        _camera = camera;
        _speed = speed;
    }

    protected override void OnExecute()
    {
        Debug.Log($"{nameof(TransitionCameraCmd)} TransitionCameraCmd to {(_camera != null ? _camera.name : "null")}");
        this.GetSystem<ICinemaChineCameraSystem>().TransitionTo(_camera, _speed);
    }
}
