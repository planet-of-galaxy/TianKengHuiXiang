using UnityEngine;
using QFramework;

// 在 CinemachineBrain 的 LateUpdate 前更新虚拟相机的位置。
[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(CharacterController))]
public class PlayerMoveController : MonoBehaviour, IController, IMove
{
    private CharacterController characterController;
    private Transform cameraTransform;
    private float cameraYaw;
    private float cameraPitch;
    private float walkSpeed;
    private float runSpeed;
    private IGamePauseSystem pauseSystem;
    private PlayerStateMachine playerStateMachine;
    private IUnRegister walkSpeedSubscription;
    private IUnRegister runSpeedSubscription;

    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField, Min(0.1f)] private float cameraDistance = 4f;
    [SerializeField] private float cameraHeight = 1.5f;
    [SerializeField] private float minimumPitch = -30f;
    [SerializeField] private float maximumPitch = 70f;
    [SerializeField, Min(0f)] private float turnSpeed = 720f;
    [SerializeField, Min(0.01f)] private float cameraCollisionRadius = 0.25f;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    // 动画使用角色自身坐标系中的移动输入。
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsMoveDisabled { get; set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        pauseSystem = this.GetSystem<IGamePauseSystem>();

        var roleContext = GetComponent<RoleContext>();
        if (roleContext == null || roleContext.ThirdViewCinema == null)
        {
            Debug.LogError("[PlayerMoveController] 角色未配置 ThirdViewCinema。", this);
            enabled = false;
            return;
        }

        cameraTransform = roleContext.ThirdViewCinema.transform;
        cameraYaw = cameraTransform.eulerAngles.y;
        cameraPitch = Mathf.Clamp(
            Mathf.DeltaAngle(0f, cameraTransform.eulerAngles.x), minimumPitch, maximumPitch);
        UpdateCameraPose();

        if (roleContext.PlayerStateMachine != null)
        {
            playerStateMachine = roleContext.PlayerStateMachine;
            playerStateMachine.RegisterState(new PlayerMoveState(this));
            playerStateMachine.ChangeState<PlayerMoveState>();
        }

        if (this.GetModel<IRoleRuntimeModel>().TryGetRoleRuntime(roleContext.RoleRuntimeIndex, out var info))
        {
            walkSpeed = info.WalkSpeed.Value;
            runSpeed = info.RunSpeed.Value;
            walkSpeedSubscription = info.WalkSpeed.Register(speed => walkSpeed = speed);
            runSpeedSubscription = info.RunSpeed.Register(speed => runSpeed = speed);
        }

        CursorUtility.Lock();
    }

    private void OnDestroy()
    {
        if (playerStateMachine != null)
        {
            playerStateMachine.UnRegisterState<PlayerMoveState>();
        }
        playerStateMachine = null;

        walkSpeedSubscription?.UnRegister();
        walkSpeedSubscription = null;
        runSpeedSubscription?.UnRegister();
        runSpeedSubscription = null;
        if (cameraTransform != null) CursorUtility.ReleaseLock();
    }

    private void Update()
    {
        if (pauseSystem != null && pauseSystem.IsPaused)
        {
            Horizontal = 0f;
            Vertical = 0f;
            IsRunning = false;
            return;
        }

        cameraYaw = Mathf.Repeat(cameraYaw + Input.GetAxis("Mouse X") * mouseSensitivity, 360f);
        cameraPitch = Mathf.Clamp(
            cameraPitch - Input.GetAxis("Mouse Y") * mouseSensitivity, minimumPitch, maximumPitch);

        if (IsMoveDisabled)
        {
            Horizontal = 0f;
            Vertical = 0f;
            IsRunning = false;
            return;
        }

        var input = Vector2.ClampMagnitude(
            new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1f);
        var cameraHeading = Quaternion.Euler(0f, cameraYaw, 0f);
        var moveDirection = cameraHeading * new Vector3(input.x, 0f, input.y);

        bool isShiftHeld = Input.GetKey(HotKeyUtility.Run) || Input.GetKey(HotKeyUtility.RunAlternate);
        IsRunning = isShiftHeld && input.sqrMagnitude > 0.001f;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, Quaternion.LookRotation(moveDirection), turnSpeed * Time.deltaTime);
        }

        var localMove = transform.InverseTransformDirection(moveDirection);
        Horizontal = localMove.x;
        Vertical = localMove.z;
        float moveSpeed = IsRunning ? runSpeed : walkSpeed;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        // 跟随重力与移动后的最终位置；角色转向不会改变独立保存的镜头朝向。
        UpdateCameraPose();
    }

    private void UpdateCameraPose()
    {
        if (cameraTransform == null) return;

        var rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
        var pivot = transform.position + Vector3.up * cameraHeight;
        var backward = rotation * Vector3.back;
        float distance = cameraDistance;

        // 忽略角色自身与触发器，遇到墙壁时缩短镜头距离。
        var hits = Physics.SphereCastAll(pivot, cameraCollisionRadius, backward,
            cameraDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            if (hit.transform.IsChildOf(transform)) continue;
            distance = Mathf.Min(distance, Mathf.Max(0.1f, hit.distance - 0.05f));
        }

        cameraTransform.SetPositionAndRotation(pivot + backward * distance, rotation);
    }
}
