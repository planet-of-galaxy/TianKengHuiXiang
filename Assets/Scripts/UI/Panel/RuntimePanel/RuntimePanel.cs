using QFramework;
using TMPro;
using UnityEngine;

/// <summary>
/// 运行时角色信息面板：展示当前角色的名称与生命值。
/// </summary>
public class RuntimePanel : UIPanel, IController
{
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _curHealth;
    [SerializeField] private TextMeshProUGUI _maxHealth;

    private RoleRuntimeModel runtimeModel;
    private RoleRuntimeInfo currentInfo;

    protected override void OnInit(IUIData uiData = null)
    {
        runtimeModel = this.GetModel<RoleRuntimeModel>();
        runtimeModel.curRole.Register(OnCurrentRoleChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
        OnCurrentRoleChanged(runtimeModel.curRole.Value);
    }

    /// <summary>
    /// 切换当前角色：更新名称（不变），并重新订阅生命值变化。
    /// </summary>
    private void OnCurrentRoleChanged(int roleId)
    {
        // 解绑上一个角色的生命值订阅
        UnbindHealth();

        if (!runtimeModel.TryGetRoleRuntime(roleId, out currentInfo))
        {
            _name.text = string.Empty;
            _curHealth.text = string.Empty;
            _maxHealth.text = string.Empty;
            return;
        }

        _name.text = currentInfo.name;
        _curHealth.text = currentInfo.CurHealth.Value.ToString();
        _maxHealth.text = currentInfo.MaxHealth.Value.ToString();

        currentInfo.CurHealth.Register(OnCurHealthChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
        currentInfo.MaxHealth.Register(OnMaxHealthChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void UnbindHealth()
    {
        if (currentInfo == null) return;

        currentInfo.CurHealth.UnRegister(OnCurHealthChanged);
        currentInfo.MaxHealth.UnRegister(OnMaxHealthChanged);
        currentInfo = null;
    }

    private void OnCurHealthChanged(float value)
    {
        _curHealth.text = value.ToString();
    }

    private void OnMaxHealthChanged(float value)
    {
        _maxHealth.text = value.ToString();
    }

    protected override void OnClose()
    {
        UnbindHealth();
    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
