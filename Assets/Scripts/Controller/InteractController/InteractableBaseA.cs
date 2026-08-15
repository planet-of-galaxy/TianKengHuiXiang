using QFramework;
using UnityEngine;

public abstract class InteractableBaseA : MonoBehaviour, IInteractableA, IController
{
    [SerializeField] private TriggerController triggerController;

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

        // 判断当前是否有带目标 tag 的物体在触发区内，决定初始启用状态
        enabled = triggerController.IsTargetInside();
    }

    private void OnDestroy()
    {
        if (triggerController == null)
        {
            return;
        }

        triggerController.OnTriggerEnterEvent.RemoveListener(OnTriggerEntered);
        triggerController.OnTriggerExitEvent.RemoveListener(OnTriggerExited);
    }

    private void OnTriggerEntered(GameObject other)
    {
        enabled = true;
        StartListening();
    }

    private void OnTriggerExited(GameObject other)
    {
        enabled = triggerController.IsTargetInside();
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
