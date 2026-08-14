using QFramework;
using UnityEngine;

public abstract class InteractableBaseA : MonoBehaviour, IInteractableA, IController
{
    public abstract void InteractA();

    public virtual float GetWeightA() => 0f;

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
