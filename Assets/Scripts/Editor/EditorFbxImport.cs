using UnityEditor;
using UnityEngine;

/// <summary>
/// 监听 Assets/Fbx/ 目录下的 FBX 导入，自动将 Material Creation Mode 设为 None。
/// 可通过菜单 Tools/FBX导入/自动设置材质模式为None 开启或关闭。
/// </summary>
public class EditorFbxImport : AssetPostprocessor
{
    private const string MenuPath  = "Tools/FBX导入/自动设置材质模式为None";
    private const string PrefsKey  = "EditorFbxImport_Enabled";
    private const string FbxFolder = "Assets/Fbx/";

    private static bool IsEnabled
    {
        get => EditorPrefs.GetBool(PrefsKey, true);
        set => EditorPrefs.SetBool(PrefsKey, value);
    }

    [MenuItem(MenuPath)]
    private static void ToggleEnabled()
    {
        IsEnabled = !IsEnabled;
        Debug.Log($"[EditorFbxImport] 自动设置材质模式：{(IsEnabled ? "已开启" : "已关闭")}");
    }

    // validate 方法负责刷新菜单勾选状态
    [MenuItem(MenuPath, true)]
    private static bool ToggleEnabledValidate()
    {
        Menu.SetChecked(MenuPath, IsEnabled);
        return true;
    }

    // 在模型导入之前调用，此时可以修改 ModelImporter 设置
    private void OnPreprocessModel()
    {
        if (!IsEnabled) return;
        if (!assetPath.StartsWith(FbxFolder)) return;

        var modelImporter = assetImporter as ModelImporter;
        if (modelImporter == null) return;

        modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
        Debug.Log($"[EditorFbxImport] {assetPath} → Material Creation Mode 已设为 None");
    }
}
