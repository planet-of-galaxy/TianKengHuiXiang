using UnityEngine;

/// <summary>轿厢外的开关门交互，沿用门窗的范围、注视和快捷键流程。</summary>
public class ElevatorDoorController : InteractableBaseA
{
    [SerializeField] private ElevatorController _elevator;

    public override void InteractA()
    {
        if (_elevator != null) _elevator.ToggleDoor();
    }

    public override float GetWeightA() => _elevator != null && !_elevator.IsTravelling ? 1f : -1f;
}
