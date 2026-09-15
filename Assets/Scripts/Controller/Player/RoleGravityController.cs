using UnityEngine;

/// <summary>独立处理角色下落与贴地，失去玩家控制后仍持续生效。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class RoleGravityController : MonoBehaviour
{
    [SerializeField] private float gravity = -15f;

    private CharacterController characterController;
    private float verticalVelocity;
    private bool grounded;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void LateUpdate()
    {
        if (!characterController.enabled || Time.deltaTime <= 0f) return;

        // 水平 Move 会更新 isGrounded，因此同时保留上一次重力移动的触地结果。
        if ((grounded || characterController.isGrounded) && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        var collisions = characterController.Move(Vector3.up * (verticalVelocity * Time.deltaTime));
        grounded = (collisions & CollisionFlags.Below) != 0;
    }
}
