using QFramework;

public interface IRoleConfigProvider : IUtility
{
    RoleConfig GetRoleConfig(int roleId);
    bool HasRole(int roleId);
    int GetRoleCount();
}