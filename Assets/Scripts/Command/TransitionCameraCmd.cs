using QFramework;
using UnityEngine;

public class TransitionCameraCmd : AbstractCommand
{
    private readonly string _cameraKey;
    private readonly float _speed;

    public TransitionCameraCmd(string cameraKey, float speed = 1f)
    {
        _cameraKey = cameraKey;
        _speed = speed;
    }

    protected override void OnExecute()
    {
        Debug.Log($"{nameof(TransitionCameraCmd)} TransitionCameraCmd to {_cameraKey}");
        this.GetSystem<ICinemaChineCameraSystem>().TransitionTo(_cameraKey, _speed);
    }
}
