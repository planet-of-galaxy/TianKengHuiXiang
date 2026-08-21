using UnityEngine;
using QFramework;

public class PlayerController : MonoBehaviour, IController
{
    private CharacterController characterController;
    private Transform cameraTransform;
    private float verticalRotation;
    private float moveSpeed;
    private float verticalVelocity;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float mouseSensitivity = 2f;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

	void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = transform.Find("CinemachineCamera");

        var runtimeModel = this.GetModel<RoleRuntimeModel>();

        void RefreshMoveSpeed()
        {
            if (runtimeModel.roleRuntimeInfo.TryGetValue(runtimeModel.curRole.Value, out var info))
            {
                moveSpeed = info.MoveSpeed.Value;
            }
        }

        RefreshMoveSpeed();
        runtimeModel.curRole.Register(_ => RefreshMoveSpeed()).UnRegisterWhenGameObjectDestroyed(gameObject);

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 第一人称视角控制
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0, mouseX, 0);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        // 重力
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // 保持贴地
        }
        verticalVelocity += gravity * Time.deltaTime;

        // 水平移动
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 horizontalMove = (transform.right * h + transform.forward * v) * moveSpeed;
        // 垂直移动（重力，不受 moveSpeed 影响）
        horizontalMove.y = verticalVelocity;
        characterController.Move(horizontalMove * Time.deltaTime);
    }
}
