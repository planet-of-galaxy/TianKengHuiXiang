using QFramework;
using UnityEngine;

public abstract class InteractableBaseA : MonoBehaviour, IInteractableA, IController
{
    [SerializeField] private TriggerController triggerController;
    [SerializeField] private BillboardController billboardController;

    public abstract void InteractA();

    public virtual float GetWeightA() => 0f;

    private void Awake()
    {
        if (triggerController == null)
        {
            return;
        }

        triggerController.OnTriggerEnterEvent.AddListener(OnTriggerEntered);
        triggerController.OnTriggerExitEvent.AddListener(OnTriggerExited);

        if (billboardController != null)
        {
            billboardController.OnBillboardFocusOn.AddListener(OnBillboardFocusedOn);
            billboardController.OnBillboardFocusOff.AddListener(OnBillboardFocusedOff);
        }

        // 判断当前是否有带目标 tag 的物体在触发区内，决定初始启用状态
        billboardController.gameObject.SetActive(triggerController.IsTargetInside());
    }

    private void OnDestroy()
    {
        if (triggerController == null)
        {
            return;
        }

        triggerController.OnTriggerEnterEvent.RemoveListener(OnTriggerEntered);
        triggerController.OnTriggerExitEvent.RemoveListener(OnTriggerExited);

        if (billboardController != null)
        {
            billboardController.OnBillboardFocusOn.RemoveListener(OnBillboardFocusedOn);
            billboardController.OnBillboardFocusOff.RemoveListener(OnBillboardFocusedOff);
        }
    }

    private void OnTriggerEntered(GameObject other)
    {
        if (billboardController != null)
        {
            billboardController.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExited(GameObject other)
    {
        if (billboardController != null)
        {
            billboardController.gameObject.SetActive(false);
        }
    }

    private void OnBillboardFocusedOn()
    {
        StartListening();
    }

    private void OnBillboardFocusedOff()
    {
        StopListening();
    }

    public void StartListening()
    {
        this.SendCommand(new ListeningControlCmd(this, true));
    }

    public void StopListening()
    {
        this.SendCommand(new ListeningControlCmd(this, false));
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
