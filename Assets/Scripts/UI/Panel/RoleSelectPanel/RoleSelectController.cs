using QFramework;
using UnityEngine;

[RequireComponent(typeof(RoleSelectPanel))]
public class RoleSelectController : MonoBehaviour, IController
{
    private RoleSelectPanel _panel;

    void Awake()
    {
        _panel = GetComponent<RoleSelectPanel>();
        _panel.OnRoleSelectItemHover += OnItemHover;
        _panel.OnRoleSelectItemClicked += OnItemClicked;
    }

    void OnDestroy()
    {
        if (_panel != null)
        {
            _panel.OnRoleSelectItemHover -= OnItemHover;
            _panel.OnRoleSelectItemClicked -= OnItemClicked;
        }
    }

    private void OnItemHover(int runtimeIndex)
    {
        // TODO: 预览对应角色（如切换虚拟相机、高亮等）
        Debug.Log($"[RoleSelect] Hover runtimeIndex={runtimeIndex}");
    }

    private void OnItemClicked(int runtimeIndex)
    {
        this.GetSystem<IRoleRuntimeSystem>().SetCurrentRole(runtimeIndex);
    }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
