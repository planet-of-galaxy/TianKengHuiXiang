using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class InteractSystem : MonoSingleton<InteractSystem>
{
    private readonly List<IInteractableA> _interactables = new();

    public void AddTarget(IInteractableA interactable)
    {
        if (interactable == null || _interactables.Contains(interactable))
        {
            return;
        }

        _interactables.Add(interactable);
    }

    public void RemoveTarget(IInteractableA interactable)
    {
        _interactables.Remove(interactable);
    }

    private void Update()
    {
        if (_interactables.Count == 0)
        {
            return;
        }

        IInteractableA target = _interactables[0];
        if (_interactables.Count > 1)
        {
            for (int i = 1; i < _interactables.Count; i++)
            {
                if (_interactables[i].GetWeightA() > target.GetWeightA())
                {
                    target = _interactables[i];
                }
            }
        }

        // 交互快捷键统一由 HotKeyUtility.InteractA 提供（默认 E），交互物可重写覆盖
        if (!Input.GetKeyDown(target.InteractKey))
        {
            return;
        }

        target.InteractA();
    }
}
