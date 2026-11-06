using System.Collections;
using UnityEngine;

public class DoorController : InteractableBaseA
{
    [SerializeField] private GameObject _doorLeaf;
    // 可选的另一扇门/窗：双扇结构中与 _doorLeaf 同步开关，绕各自铰链反向旋转；单扇结构留空。
    [Tooltip("另一扇门/窗的铰链对象，与主扇同步反向旋转。用于双扇门窗，单扇时留空。")]
    [SerializeField] private GameObject _oppositeDoorLeaf;
    [SerializeField] private float _rotationDuration = 0.5f;

    private Quaternion _closedRotation;
    private Quaternion _oppositeClosedRotation;
    private Coroutine _rotationCoroutine;
    private bool _isOpenTarget;
    private bool _isInitialized;

    private void Start()
    {
        InitializeDoor();
    }

    public override void InteractA()
    {
        if (_doorLeaf == null)
        {
            Debug.LogWarning("[DoorController] Door leaf is not assigned");
            return;
        }

        InitializeDoor();

        if (_rotationCoroutine != null)
        {
            StopCoroutine(_rotationCoroutine);
        }

        _isOpenTarget = !_isOpenTarget;
        Quaternion targetRotation;
        Quaternion oppositeTargetRotation = _oppositeClosedRotation;

        if (_isOpenTarget)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                _isOpenTarget = false;
                Debug.LogWarning("[DoorController] Main camera was not found");
                return;
            }

            float rotationDirection = GetOpenDirection(mainCamera.transform.position);
            targetRotation = _closedRotation * Quaternion.Euler(0f, 90f * rotationDirection, 0f);
            oppositeTargetRotation = _oppositeClosedRotation * Quaternion.Euler(0f, -90f * rotationDirection, 0f);
        }
        else
        {
            targetRotation = _closedRotation;
        }

        _rotationCoroutine = StartCoroutine(RotateDoor(targetRotation, oppositeTargetRotation));
    }

    private void InitializeDoor()
    {
        if (_isInitialized || _doorLeaf == null)
        {
            return;
        }

        _closedRotation = _doorLeaf.transform.localRotation;
        if (_oppositeDoorLeaf != null)
        {
            _oppositeClosedRotation = _oppositeDoorLeaf.transform.localRotation;
        }
        _isInitialized = true;
    }

    private float GetOpenDirection(Vector3 cameraPosition)
    {
        Transform doorTransform = _doorLeaf.transform;
        Vector3 doorToCamera = Vector3.ProjectOnPlane(
            cameraPosition - doorTransform.position,
            doorTransform.up
        );

        Vector3 closedRight = _closedRotation * Vector3.right;
        if (doorTransform.parent != null)
        {
            closedRight = doorTransform.parent.TransformDirection(closedRight);
        }

        float side = Vector3.Dot(
            Vector3.Cross(doorToCamera.normalized, closedRight.normalized),
            doorTransform.up
        );
        return side >= 0f ? 1f : -1f;
    }

    private IEnumerator RotateDoor(Quaternion targetRotation, Quaternion oppositeTargetRotation)
    {
        if (_rotationDuration <= 0f)
        {
            _doorLeaf.transform.localRotation = targetRotation;
            if (_oppositeDoorLeaf != null)
            {
                _oppositeDoorLeaf.transform.localRotation = oppositeTargetRotation;
            }
            _rotationCoroutine = null;
            yield break;
        }

        float rotationSpeed = 90f / _rotationDuration;
        while (Quaternion.Angle(_doorLeaf.transform.localRotation, targetRotation) > 0.01f
               || (_oppositeDoorLeaf != null
                   && Quaternion.Angle(_oppositeDoorLeaf.transform.localRotation, oppositeTargetRotation) > 0.01f))
        {
            _doorLeaf.transform.localRotation = Quaternion.RotateTowards(
                _doorLeaf.transform.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            if (_oppositeDoorLeaf != null)
            {
                _oppositeDoorLeaf.transform.localRotation = Quaternion.RotateTowards(
                    _oppositeDoorLeaf.transform.localRotation,
                    oppositeTargetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            yield return null;
        }

        _doorLeaf.transform.localRotation = targetRotation;
        if (_oppositeDoorLeaf != null)
        {
            _oppositeDoorLeaf.transform.localRotation = oppositeTargetRotation;
        }
        _rotationCoroutine = null;
    }
}
