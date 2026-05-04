using QFramework;

public interface IRoleSystem : ISystem
{
    RoleConfig GetRoleConfig(int roleId);
    bool HasRole(int roleId);
    int GetRoleCount();
}
