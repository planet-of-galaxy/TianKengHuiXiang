using QFramework;

public interface IToolConfigProvider : IUtility
{
    ToolConfig GetToolConfig(int toolId);
    bool HasTool(int toolId);
    int GetToolCount();
}
