using QFramework;
using UnityEngine;

[RequireComponent(typeof(RoleContext))]
public class RoleObserveListener : MonoBehaviour, IController
{
    private RoleContext _roleContext;
    private IUnRegister _unRegister;

    void Awake()
    {
        _roleContext = GetComponent<RoleContext>();
    }

    void OnEnable()
    {
        _unRegister = this.RegisterEvent<RoleObserveEvent>(OnRoleObserve);
    }

    void OnDisable()
    {
        _unRegister?.UnRegister();
        _unRegister = null;
    }

    private void OnRoleObserve(RoleObserveEvent e)
    {
        if (_roleContext == null) return;
        if (e.RuntimeIndex != _roleContext.roleRuntimeIndex) return;

        if (_roleContext.observeCinema == null)
        {
            Debug.LogError($"{nameof(RoleObserveListener)}: roleRuntimeIndex={_roleContext.roleRuntimeIndex} 的 observeCinema 未配置");
            return;
        }

        this.GetSystem<ICinemaChineCameraSystem>().TransitionTo(_roleContext.observeCinema);
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
