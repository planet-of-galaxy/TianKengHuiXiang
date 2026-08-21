using QFramework;

public interface IRoleRuntimeSystem : ISystem
{
    int CreateRole(int roleId);
    void SetCurrentRole(int roleRuntimeId);
}
