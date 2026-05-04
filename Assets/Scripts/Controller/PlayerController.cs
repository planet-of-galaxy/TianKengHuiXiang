using UnityEngine;
using QFramework;

public class PlayerController : MonoBehaviour, IController
{
    private CharacterController characterController;
    private Transform cameraTransform;
    private float verticalRotation;
    private float moveSpeed;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = transform.Find("CinemachineCamera");

        var model = this.GetModel<PlayerDataModel>();
        moveSpeed = model.CurInfo.Value.moveSpeed;
        model.CurInfo.Register(info => moveSpeed = info.moveSpeed).UnRegisterWhenGameObjectDestroyed(gameObject);

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * 2f;
        float mouseY = Input.GetAxis("Mouse Y") * 2f;

        transform.Rotate(0, mouseX, 0);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }
}
