using UnityEngine;

public class DoorController : InteractableBaseA
{
    public override void InteractA()
    {
        Debug.Log("[DoorController] Interact triggered");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartListening();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopListening();
        }
    }
}
