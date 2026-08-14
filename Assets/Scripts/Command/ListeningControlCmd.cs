using QFramework;

public class ListeningControlCmd : AbstractCommand
{
    private readonly IInteractableA _interactable;
    private readonly bool _startListening;

    public ListeningControlCmd(IInteractableA interactable, bool startListening)
    {
        _interactable = interactable;
        _startListening = startListening;
    }

    protected override void OnExecute()
    {
        if (_startListening)
        {
            InteractSystem.Instance.AddTarget(_interactable);
        }
        else
        {
            InteractSystem.Instance.RemoveTarget(_interactable);
        }
    }
}
