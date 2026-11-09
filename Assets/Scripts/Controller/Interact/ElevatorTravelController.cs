using UnityEngine;

/// <summary>轿厢内的下一楼层交互。</summary>
public class ElevatorTravelController : InteractableBaseA
{
    [SerializeField] private ElevatorController _elevator;

    public override void InteractA()
    {
        if (_elevator != null) _elevator.GoToNextFloor();
    }

    public override float GetWeightA() => _elevator != null && !_elevator.IsTravelling ? 1f : -1f;
}
