using UnityEngine;
using QFramework;

public class PlayerMoveController : MonoBehaviour, IController
{
    private CharacterController characterController; // 执行角色移动并处理碰撞的组件。
    private Transform cameraTransform; // 第一人称摄像机的 Transform，用于调整俯仰角度。
    private float verticalRotation; // 当前摄像机俯仰角度，限制在 -90 到 90 度之间。
    private float walkSpeed; // 角色当前走路速度，与运行时角色数据同步。
    private float runSpeed; // 角色当前奔跑速度，按住 Shift 且向前移动时使用。
    private IGamePauseSystem pauseSystem; // 查询游戏暂停状态，暂停时屏蔽移动和视角输入。
    private PlayerStateMachine playerStateMachine;
    private PlayerMoveState playerMoveState;
    private IUnRegister walkSpeedSubscription; // 走路速度变化的订阅句柄，销毁时取消订阅。
    private IUnRegister runSpeedSubscription; // 奔跑速度变化的订阅句柄，销毁时取消订阅。
    [SerializeField] private float mouseSensitivity = 2f; // 鼠标控制视角旋转的灵敏度，可在 Inspector 中调整。

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }

	void Awake()
    {
        characterController = GetComponent<CharacterController>();
        pauseSystem = this.GetSystem<IGamePauseSystem>();

        var roleContext = GetComponent<RoleContext>();
        if (roleContext != null && roleContext.PlayerStateMachine != null)
        {
            playerStateMachine = roleContext.PlayerStateMachine;
            playerMoveState = new PlayerMoveState(this);
            playerStateMachine.RegisterState(playerMoveState);
        }

        if (roleContext != null && roleContext.FirstViewCinema != null)
        {
            cameraTransform = roleContext.FirstViewCinema.transform;
        }

        if (roleContext != null
            && this.GetModel<IRoleRuntimeModel>().TryGetRoleRuntime(roleContext.RoleRuntimeIndex, out var info))
        {
            // Register 不会立刻回调一次，当前值要自己读。
            walkSpeed = info.WalkSpeed.Value;
            runSpeed = info.RunSpeed.Value;
            walkSpeedSubscription = info.WalkSpeed.Register(speed => walkSpeed = speed);
            runSpeedSubscription = info.RunSpeed.Register(speed => runSpeed = speed);
        }

        CursorUtility.Lock();
    }

    void OnDestroy()
    {
        if (playerStateMachine != null && playerMoveState != null)
        {
            playerStateMachine.UnRegisterState(playerMoveState);
        }
        playerMoveState = null;
        playerStateMachine = null;

        walkSpeedSubscription?.UnRegister();
        walkSpeedSubscription = null;
        runSpeedSubscription?.UnRegister();
        runSpeedSubscription = null;
        CursorUtility.ReleaseLock();
    }

    void Update()
    {
        // 游戏暂停（打开背包/仓库等界面）时屏蔽第一人称视角旋转与移动输入，
        // 避免鼠标移到 UI 外时带动镜头转动
        if (pauseSystem != null && pauseSystem.IsPaused)
        {
            return;
        }

        // 第一人称视角控制
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0, mouseX, 0);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        // 水平移动（A/D，沿用轴输入）
        Horizontal = Input.GetAxis("Horizontal");
        // 前后移动（W/S，沿用轴输入）
        Vertical = Input.GetAxis("Vertical");
        bool isShiftHeld = Input.GetKey(HotKeyUtility.Run) || Input.GetKey(HotKeyUtility.RunAlternate);
        float moveSpeed = isShiftHeld && Vertical > 0f ? runSpeed : walkSpeed;
        Vector3 horizontalMove = (transform.right * Horizontal + transform.forward * Vertical) * moveSpeed;
        characterController.Move(horizontalMove * Time.deltaTime);
    }
}
