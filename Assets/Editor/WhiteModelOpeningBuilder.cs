using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Builds wall assemblies from the original white-model prefab variants.</summary>
public static class WhiteModelOpeningBuilder
{
    public const string OutputFolder = "Assets/Resources/Prefabe/WhiteModel/门窗组合";
    private const string ModelFolder = "Assets/Resources/Prefabe/WhiteModel";
    private const string InteractionTemplate = "Assets/Prefabe/House/Wall/Door/door_frame.prefab";

    [MenuItem("Tools/White Model/Build Door and Window Assemblies")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Build assemblies outside Play Mode.");

        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder(ModelFolder, "门窗组合");

        var template = AssetDatabase.LoadAssetAtPath<GameObject>(InteractionTemplate);
        BuildOne("门墙组合", "门洞墙", "门嵌件", true, -0.4f, template);
        BuildOne("小窗墙组合_0.6×0.6m", "小窗洞墙_0.6×0.6m", "小窗_0.6×0.6m嵌件", true, 0f, template);
        BuildOne("中窗墙组合_1×1m", "中窗洞墙_1×1m", "中窗_1×1m嵌件", true, 0f, template);
        BuildOne("大窗墙组合_1.4×1.2m", "大窗洞墙_1.4×1.2m", "大窗_1.4×1.2m嵌件", true, 0f, template);
        BuildOne("通风口墙组合", "通风口洞墙", "通风口嵌件", false, 1.05f, template);
        AssetDatabase.SaveAssets();
        Debug.Log("[WhiteModelOpeningBuilder] Built 5 assemblies (4 interactive, 1 fixed vent).");
    }

    private static void BuildOne(string name, string wallName, string insertName,
        bool interactive, float promptHeight, GameObject template)
    {
        // A prefab contents scene keeps generated objects out of the user's open scene.
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        var root = new GameObject(name);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
        try
        {
            InstantiateModel($"{ModelFolder}/洞口墙/{wallName}.prefab", root.transform);
            var insert = InstantiateModel($"{ModelFolder}/门窗与通风口/{insertName}.prefab", root.transform);
            // Walls are centred at y=0; inserts were authored relative to the floor.
            insert.transform.localPosition = new Vector3(0f, -1.5f, 0f);

            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                if (filter.GetComponent<MeshCollider>() == null)
                    filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
            }

            if (interactive)
            {
                var leaves = insert.GetComponentsInChildren<Transform>(true)
                    .Where(t => t.name.EndsWith("扇_部件", StringComparison.Ordinal)).ToArray();
                var primary = leaves.FirstOrDefault(t => t.name.Contains("左扇")) ?? leaves.FirstOrDefault();
                if (primary == null)
                    throw new InvalidOperationException($"No opening leaf found in {insertName}");
                var opposite = leaves.FirstOrDefault(t => t != primary);
                if (leaves.Length > 2)
                    throw new InvalidOperationException($"Unexpected number of leaves in {insertName}");

                var templateTrigger = template.GetComponentInChildren<TriggerController>(true);
                var templateBillboard = template.GetComponentInChildren<BillboardController>(true);
                var trigger = UnityEngine.Object.Instantiate(templateTrigger, root.transform);
                trigger.name = "InteractionTrigger";
                trigger.transform.localPosition = Vector3.zero;
                trigger.transform.localRotation = Quaternion.identity;
                trigger.transform.localScale = Vector3.one;
                var box = trigger.GetComponent<BoxCollider>();
                box.center = new Vector3(0f, promptHeight, 0f);
                box.size = new Vector3(4f, 3f, 6f);
                box.isTrigger = true;

                var billboard = UnityEngine.Object.Instantiate(templateBillboard, root.transform);
                billboard.name = "Canvas";
                billboard.transform.localPosition = new Vector3(0f, promptHeight, 0f);
                billboard.transform.localRotation = Quaternion.identity;
                billboard.transform.localScale = Vector3.one;

                var controller = root.AddComponent<DoorController>();
                var serialized = new SerializedObject(controller);
                serialized.FindProperty("triggerController").objectReferenceValue = trigger;
                serialized.FindProperty("billboardController").objectReferenceValue = billboard;
                serialized.FindProperty("_doorLeaf").objectReferenceValue = primary.gameObject;
                serialized.FindProperty("_oppositeDoorLeaf").objectReferenceValue = opposite != null ? opposite.gameObject : null;
                serialized.FindProperty("_rotationDuration").floatValue = 0.5f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{OutputFolder}/{name}.prefab", out bool success);
            if (!success || prefab == null)
                throw new InvalidOperationException($"Failed to save {name}");
        }
        finally
        {
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    private static GameObject InstantiateModel(string path, Transform parent)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) throw new InvalidOperationException($"Missing prefab: {path}");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        return instance;
    }
}
