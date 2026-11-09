using System.Collections.Generic;
using UnityEngine;

/// <summary>管理滑门、楼层顺序和轿厢升降；楼层标记的位置对应轿厢原点。</summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-200)]
public class ElevatorController : MonoBehaviour
{
    [Header("轿厢与楼层")]
    [SerializeField] private Transform _cabin;
    [Tooltip("按停靠顺序拖入楼层标记，最后一层的下一层为第一层。标记必须放在轿厢外。")]
    [SerializeField] private List<Transform> _floors = new();
    [SerializeField, Min(0.01f)] private float _moveSpeed = 2f;
    [SerializeField] private TriggerController _passengerTrigger;

    [Header("双侧滑门")]
    [SerializeField] private Transform _leftDoor;
    [SerializeField] private Transform _rightDoor;
    [Tooltip("两扇门在各自父对象局部坐标系中的滑动方向。")]
    [SerializeField] private Vector3 _doorSlideAxis = Vector3.right;
    [SerializeField, Min(0f)] private float _doorSlideDistance = 0.8f;
    [SerializeField, Min(0f)] private float _doorDuration = 0.5f;

    private enum TravelState { Idle, Closing, Moving, Opening }
    private TravelState _state;
    private Vector3 _leftClosedPosition;
    private Vector3 _rightClosedPosition;
    private Vector3 _destination;
    private float _doorProgress;
    private bool _doorOpenTarget;
    private bool _initialized;
    private readonly Dictionary<GameObject, Transform> _occupants = new();
    private readonly HashSet<Transform> _movedPassengers = new();

    public bool IsTravelling => _state != TravelState.Idle;
    public bool IsDoorOpen => _doorOpenTarget;

    private void Awake()
    {
        Initialize();
        if (_passengerTrigger == null) return;
        _passengerTrigger.OnTriggerEnterEvent.AddListener(OnPassengerEntered);
        _passengerTrigger.OnTriggerExitEvent.AddListener(OnPassengerExited);
    }

    private void OnDestroy()
    {
        if (_passengerTrigger == null) return;
        _passengerTrigger.OnTriggerEnterEvent.RemoveListener(OnPassengerEntered);
        _passengerTrigger.OnTriggerExitEvent.RemoveListener(OnPassengerExited);
    }

    private bool Initialize()
    {
        if (_initialized) return true;
        if (_cabin == null || _leftDoor == null || _rightDoor == null) return false;
        _leftClosedPosition = _leftDoor.localPosition;
        _rightClosedPosition = _rightDoor.localPosition;
        _initialized = true;
        return true;
    }

    public void ToggleDoor()
    {
        if (!isActiveAndEnabled || IsTravelling || !Initialize()) return;
        _doorOpenTarget = !_doorOpenTarget;
    }

    public void GoToNextFloor()
    {
        if (!isActiveAndEnabled || IsTravelling || !Initialize()) return;

        int current = -1;
        float nearestDistance = float.PositiveInfinity;
        for (int i = 0; i < _floors.Count; i++)
        {
            if (!IsValidFloor(_floors[i])) continue;
            float distance = (_floors[i].position - _cabin.position).sqrMagnitude;
            if (distance >= nearestDistance) continue;
            nearestDistance = distance;
            current = i;
        }

        if (current >= 0)
        {
            for (int offset = 1; offset < _floors.Count; offset++)
            {
                Transform floor = _floors[(current + offset) % _floors.Count];
                if (!IsValidFloor(floor)
                    || (floor.position - _cabin.position).sqrMagnitude <= 0.000001f) continue;

                // 在出发时保存世界坐标，避免移动中的标记改变本次行程。
                _destination = floor.position;
                _doorOpenTarget = false;
                _state = TravelState.Closing;
                return;
            }
        }

        Debug.LogWarning("[ElevatorController] 请配置至少两个不同位置的楼层标记，且不要将标记放在轿厢下。", this);
    }

    private bool IsValidFloor(Transform floor)
    {
        return floor != null && !floor.IsChildOf(_cabin);
    }

    private void Update()
    {
        if (!Initialize()) return;
        float target = _doorOpenTarget ? 1f : 0f;
        _doorProgress = _doorDuration <= 0f
            ? target
            : Mathf.MoveTowards(_doorProgress, target, Time.deltaTime / _doorDuration);
        Vector3 offset = _doorSlideAxis.normalized * (_doorSlideDistance * _doorProgress);
        _leftDoor.localPosition = _leftClosedPosition - offset;
        _rightDoor.localPosition = _rightClosedPosition + offset;

        switch (_state)
        {
            case TravelState.Closing:
                if (_doorProgress == 0f) _state = TravelState.Moving;
                break;
            case TravelState.Moving:
                Vector3 nextPosition = Vector3.MoveTowards(
                    _cabin.position, _destination, Mathf.Max(0.01f, _moveSpeed) * Time.deltaTime);
                MoveCabin(nextPosition);
                if ((_cabin.position - _destination).sqrMagnitude <= 0.000001f)
                {
                    MoveCabin(_destination);
                    _doorOpenTarget = true;
                    _state = TravelState.Opening;
                }
                break;
            case TravelState.Opening:
                if (_doorProgress == 1f) _state = TravelState.Idle;
                break;
        }
    }

    private void MoveCabin(Vector3 position)
    {
        Vector3 delta = position - _cabin.position;
        if (delta == Vector3.zero) return;
        _cabin.position = position;
        _movedPassengers.Clear();
        foreach (var occupant in _occupants)
        {
            Transform passenger = occupant.Value;
            if (occupant.Key == null || passenger == null || passenger.IsChildOf(_cabin)
                || !_movedPassengers.Add(passenger)) continue;

            // 直接同步位移，避免向下运行时 CharacterController.Move 被旧地板阻挡。
            passenger.position += delta;
        }
        Physics.SyncTransforms();
    }

    private void OnPassengerEntered(GameObject other)
    {
        var character = other.GetComponentInParent<CharacterController>();
        _occupants[other] = character != null ? character.transform : other.transform;
    }

    private void OnPassengerExited(GameObject other)
    {
        _occupants.Remove(other);
    }
}
