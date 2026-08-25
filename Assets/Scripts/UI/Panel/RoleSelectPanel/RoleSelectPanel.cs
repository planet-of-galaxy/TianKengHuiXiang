using System;
using QFramework;
using UnityEngine;

public class RoleSelectPanel : UIPanel, IController
{
    [SerializeField] private Transform _itemContainer;

    private const string ItemPrefabPath = "UI/Panel/RoleSelectPanel/RoleSelectItem";

    public event Action<int> OnRoleSelectItemHover;
    public event Action<int> OnRoleSelectItemClicked;

    protected override void OnInit(IUIData uiData = null)
    {
        var model = this.GetModel<RoleRuntimeModel>();
        var prefab = Resources.Load<GameObject>(ItemPrefabPath);

        foreach (var info in model.GetAllRoleRuntimes())
        {
            var go = Instantiate(prefab, _itemContainer);
            var item = go.GetComponent<RoleSelectItem>();
            item.Setup(info.runtimeIndex, info.name);

            item.OnRoleSelectItemHover += runtimeIndex => OnRoleSelectItemHover?.Invoke(runtimeIndex);
            item.OnRoleSelectItemClicked += runtimeIndex => OnRoleSelectItemClicked?.Invoke(runtimeIndex);
        }
    }

    protected override void OnClose() { }

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;
}
