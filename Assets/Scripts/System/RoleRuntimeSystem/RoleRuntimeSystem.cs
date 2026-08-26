using System.Collections.Generic;
using QFramework;
using UnityEngine;

public interface IRoleRuntimeSystem : ISystem
{
    int CreateRole(int roleId);
    void SetCurrentRole(int roleRuntimeId);

    /// <summary>
    /// 当前角色实例；未生成或已销毁时为 null。
    /// </summary>
    GameObject CurrentRoleInstance { get; }

    /// <summary>
    /// 实例化当前选中的角色，返回实例；失败返回 null。
    /// </summary>
    GameObject SpawnCurrentRole(Vector3 position, Quaternion rotation);

    /// <summary>
    /// 将场景中已存在的角色设为当前角色：校验其 RoleContext 后挂载 PlayerController，不实例化新对象。
    /// 上一个当前角色只被移除 PlayerController，实例保留在场景中。失败返回 null。
    /// </summary>
    GameObject SpawnCurrentRole(GameObject roleInstance);

    /// <summary>
    /// 根据指定角色运行时 id 实例化对应角色，并移除其上所有 IController 组件，返回实例；失败返回 null。
    /// </summary>
    GameObject SpawnRoleWithoutController(int runtimeId, Vector3 position, Quaternion rotation);
}

public class RoleRuntimeSystem : AbstractSystem, IRoleRuntimeSystem
{
    private RoleRuntimeModel runtimeModel;
    private IRoleConfigProvider roleConfigProvider;
    private IJsonStorage storage;
    private RoleViewFactory viewFactory;
    private int nextRoleRuntimeId = 1;

    protected override void OnInit()
    {
        runtimeModel = this.GetModel<RoleRuntimeModel>();
        roleConfigProvider = this.GetUtility<IRoleConfigProvider>();
        storage = this.GetUtility<IJsonStorage>();
        viewFactory = new RoleViewFactory(runtimeModel, this.GetUtility<IResourceStorage>());

        Init();
    }

    /// <summary>
    /// 读取存档，将数据直接写入 RoleRuntimeModel。
    /// 若无存档则创建一个角色配置 id=0 的默认运行时实例；
    /// curRole 若无存档值或模型中不存在，则取模型第一个元素。
    /// </summary>
    private void Init()
    {
        var save = storage.Load<RoleRuntimeSaveData>("RoleRuntime");
        runtimeModel.ClearRoleRuntimes();

        var hasSave = save?.roleRuntimeDatas != null && save.roleRuntimeDatas.Count > 0;
        if (!hasSave)
        {
            CreateDefaultRole();
        }
        else
        {
            foreach (var data in save.roleRuntimeDatas)
            {
                if (data == null) continue;

                // 兼容旧存档：id 无效时补发一个实例 id
                if (data.runtimeIndex <= 0) data.runtimeIndex = AllocateRuntimeId();

                runtimeModel.AddRoleRuntime(ToRoleRuntimeInfo(data));
                nextRoleRuntimeId = Mathf.Max(nextRoleRuntimeId, data.runtimeIndex + 1);
            }
        }

        var curRole = save?.curRole ?? -1;
        if (curRole < 0 || !runtimeModel.TryGetRoleRuntime(curRole, out _))
        {
            foreach (var info in runtimeModel.GetAllRoleRuntimes())
            {
                curRole = info.runtimeIndex;
                break;
            }
        }

        runtimeModel.curRole.Value = curRole;

        // 无存档时刚创建了默认角色，立即落盘，保证首帧后存档一致
        if (!hasSave)
        {
            SaveRoleRuntime();
        }
    }

    private int AllocateRuntimeId()
    {
        return nextRoleRuntimeId++;
    }

    private void CreateDefaultRole()
    {
        var info = new RoleRuntimeInfo
        {
            runtimeIndex = AllocateRuntimeId(),
            configId = 0,
        };
        var roleConfig = roleConfigProvider.GetRoleConfig(0);
        if (roleConfig != null)
        {
            info.name = roleConfig.name;
            info.CurHealth.Value = roleConfig.health;
            info.MaxHealth.Value = roleConfig.health;
        }
        // moveSpeed 是静态配置，不随存档变化，直接从配置取
        info.MoveSpeed.Value = roleConfig?.moveSpeed ?? 0f;

        runtimeModel.AddRoleRuntime(info);
    }

    /// <summary>
    /// 将持久化数据转换为运行时信息并写入模型。
    /// </summary>
    private RoleRuntimeInfo ToRoleRuntimeInfo(RoleRuntimeData data)
    {
        var info = new RoleRuntimeInfo
        {
            runtimeIndex = data.runtimeIndex,
            configId = data.configId,
        };
        info.CurHealth.Value = data.curHealth;
        info.MaxHealth.Value = data.maxHealth;

        // moveSpeed 是静态配置，不随存档变化，直接从配置取
        var config = roleConfigProvider.GetRoleConfig(data.configId);
        info.MoveSpeed.Value = config?.moveSpeed ?? 0f;
        info.name = config?.name;
        return info;
    }

    /// <summary>
    /// 创建一个新的角色运行时实例，返回其实例 id（调用方可用它 SetCurrentRole）。
    /// </summary>
    public int CreateRole(int roleId)
    {
        var roleConfig = roleConfigProvider.GetRoleConfig(roleId);
        if (roleConfig == null)
        {
            Debug.LogError($"[RoleRuntimeSystem] RoleConfig not found for roleId: {roleId}");
            return -1;
        }

        var info = new RoleRuntimeInfo
        {
            runtimeIndex = AllocateRuntimeId(),
            configId = roleId,
        };
        info.name = roleConfig.name;
        info.CurHealth.Value = roleConfig.health;
        info.MaxHealth.Value = roleConfig.health;
        info.MoveSpeed.Value = roleConfig.moveSpeed;

        runtimeModel.AddRoleRuntime(info);

        SaveRoleRuntime();

        return info.runtimeIndex;
    }

    public void SetCurrentRole(int roleRuntimeId)
    {
        if (!runtimeModel.TryGetRoleRuntime(roleRuntimeId, out _))
        {
            Debug.LogWarning($"[RoleRuntimeSystem] RoleRuntimeInfo not found for id: {roleRuntimeId}");
            return;
        }

        runtimeModel.curRole.Value = roleRuntimeId;
        SaveRoleRuntime();
    }

    public GameObject CurrentRoleInstance => viewFactory.CurrentRoleInstance;

    /// <summary>
    /// 实例化当前选中的角色，返回实例；失败返回 null。
    /// </summary>
    public GameObject SpawnCurrentRole(Vector3 position, Quaternion rotation)
    {
        return viewFactory.SpawnCurrentRole(position, rotation);
    }

    /// <summary>
    /// 将场景中已存在的角色设为当前角色：校验其 RoleContext 后挂载 PlayerController，不实例化新对象。
    /// 上一个当前角色只被移除 PlayerController，实例保留在场景中。失败返回 null。
    /// </summary>
    public GameObject SpawnCurrentRole(GameObject roleInstance)
    {
        return viewFactory.SpawnCurrentRole(roleInstance);
    }

    /// <summary>
    /// 根据指定角色运行时 id 实例化对应角色，并移除其上所有 IController 组件，返回实例；失败返回 null。
    /// </summary>
    public GameObject SpawnRoleWithoutController(int runtimeId, Vector3 position, Quaternion rotation)
    {
        return viewFactory.SpawnRoleWithoutController(runtimeId, position, rotation);
    }

    /// <summary>
    /// 从 RoleRuntimeModel 中读取数据并保存。
    /// </summary>
    private void SaveRoleRuntime()
    {
        var save = new RoleRuntimeSaveData
        {
            roleRuntimeDatas = new List<RoleRuntimeData>(),
            curRole = runtimeModel.curRole.Value,
        };
        foreach (var info in runtimeModel.GetAllRoleRuntimes())
        {
            save.roleRuntimeDatas.Add(ToRoleRuntimeData(info));
        }
        storage.Save(save, "RoleRuntime");
    }

    /// <summary>
    /// 将模型中的运行时信息转换为持久化数据。
    /// </summary>
    private RoleRuntimeData ToRoleRuntimeData(RoleRuntimeInfo info)
    {
        return new RoleRuntimeData
        {
            runtimeIndex = info.runtimeIndex,
            configId = info.configId,
            curHealth = info.CurHealth.Value,
            maxHealth = info.MaxHealth.Value,
        };
    }
}
