using QFramework;
using UnityEngine;

[RequireComponent(typeof(RoleContext))]
public class CurrentRoleSetListener : MonoBehaviour, IController
{
    private RoleContext _roleContext;
    private IUnRegister _unRegister;

    void Awake()
    {
        _roleContext = GetComponent<RoleContext>();
    }

    void OnEnable()
    {
        _unRegister = this.RegisterEvent<CurrentRoleSetEvent>(OnCurrentRoleSet);
    }

    void OnDisable()
    {
        _unRegister?.UnRegister();
        _unRegister = null;
    }

    private void OnCurrentRoleSet(CurrentRoleSetEvent e)
    {
        if (_roleContext == null) return;
        if (e.RuntimeIndex != _roleContext.roleRuntimeIndex) return;

        this.GetSystem<IRoleRuntimeSystem>().SpawnCurrentRole(gameObject);
        this.GetSystem<ICinemaChineCameraSystem>().TransitionTo(_roleContext.firstViewCinema);
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
