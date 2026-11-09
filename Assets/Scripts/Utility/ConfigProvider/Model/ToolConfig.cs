[System.Serializable]
public class ToolConfig
{
    public int toolId;
    public string name;

    /// <summary>工具图标的 Resources 相对路径，不包含扩展名。</summary>
    public string icon;

    /// <summary>工具模型预制体的 Resources 相对路径，不包含扩展名。</summary>
    public string prefab;
}
