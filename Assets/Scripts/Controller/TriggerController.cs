using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class TriggerController : MonoBehaviour
{
    [SerializeField] private string targetTag;

    public UnityEvent<GameObject> OnTriggerEnterEvent = new();
    public UnityEvent<GameObject> OnTriggerExitEvent = new();

    private BoxCollider boxCollider;
    private readonly HashSet<GameObject> insideTargets = new();

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (MatchTag(other) && insideTargets.Add(other.gameObject))
        {
            OnTriggerEnterEvent?.Invoke(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (insideTargets.Remove(other.gameObject))
        {
            OnTriggerExitEvent?.Invoke(other.gameObject);
        }
    }

    private bool MatchTag(Collider other)
    {
        return string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag);
    }

    /// <summary>当前是否有带目标 tag 的物体处于触发区内。</summary>
    public bool IsTargetInside()
    {
        return insideTargets.Count > 0;
    }
}
