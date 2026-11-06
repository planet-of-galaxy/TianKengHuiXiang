using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Organizes Test's construction models without unpacking their prefabs.</summary>
public static class TestSceneOrganizer
{
    private const string ScenePath = "Assets/Scripts/Test/Test.unity";
    private const string ModelPath = "Assets/Resources/Prefabe/WhiteModel/";
    private const string RootName = "场景搭建";

    [MenuItem("Tools/White Model/Organize Test Scene")]
    public static void Organize()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play Mode before organizing Test.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            throw new InvalidOperationException("Open " + ScenePath + " before organizing.");

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Organize Test Scene");
        var root = scene.GetRootGameObjects().FirstOrDefault(go => go.name == RootName);
        var models = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<Transform>(true))
            .Select(t => t.gameObject)
            .Where(PrefabUtility.IsAnyPrefabInstanceRoot)
            .Where(go => PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go)
                .Replace('\\', '/').StartsWith(ModelPath, StringComparison.Ordinal))
            // Include scene-added models parented under another instance (e.g. fences),
            // while keeping prefab-authored wall/insert assemblies intact.
            .Where(go => PrefabUtility.GetOutermostPrefabInstanceRoot(go) == go ||
                PrefabUtility.IsAddedGameObjectOverride(go))
            .Distinct().ToArray();
        if (models.Length == 0)
            throw new InvalidOperationException("No construction models found in Test.");

        root = root ?? CreateGroup(RootName, null, scene);
        if (root.transform.position != Vector3.zero || root.transform.rotation != Quaternion.identity ||
            root.transform.lossyScale != Vector3.one)
            throw new InvalidOperationException("The construction root must have an identity transform.");
        SetStatic(root, false);
        foreach (var category in models.GroupBy(Category).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var parent = root.transform.Find(category.Key);
            if (parent == null) parent = CreateGroup(category.Key, root.transform, scene).transform;
            SetStatic(parent.gameObject, !category.Key.StartsWith("可交互", StringComparison.Ordinal));
            var counters = new Dictionary<string, int>();
            foreach (var model in category.OrderBy(AssetName, StringComparer.Ordinal)
                .ThenBy(go => go.transform.position.x).ThenBy(go => go.transform.position.z)
                .ThenBy(go => go.transform.position.y).ThenBy(go => go.GetInstanceID()))
            {
                var position = model.transform.position;
                var rotation = model.transform.rotation;
                var scale = model.transform.lossyScale;
                Undo.SetTransformParent(model.transform, parent, "Group model");
                string type = AssetName(model);
                counters.TryGetValue(type, out int count);
                counters[type] = ++count;
                Undo.RecordObject(model, "Name model");
                model.name = type + "_" + count.ToString("D3");
                PrefabUtility.RecordPrefabInstancePropertyModifications(model);
                ConfigureStatic(model);
                if (Vector3.Distance(position, model.transform.position) > 0.0001f ||
                    Quaternion.Angle(rotation, model.transform.rotation) > 0.001f ||
                    Vector3.Distance(scale, model.transform.lossyScale) > 0.0001f)
                    throw new InvalidOperationException("Model transform changed: " + model.name);
            }
        }
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[TestSceneOrganizer] Organized " + models.Length + " models: " +
            string.Join(", ", models.GroupBy(Category).Select(g => g.Key + "=" + g.Count())));
    }

    private static string AssetName(GameObject go) => System.IO.Path.GetFileNameWithoutExtension(
        PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go));

    private static string Category(GameObject go)
    {
        string name = AssetName(go);
        if (go.GetComponentInChildren<InteractableBaseA>(true) != null)
        {
            if (name.Contains("窗")) return "可交互窗";
            if (name.Contains("门")) return "可交互门";
            return "其他交互物";
        }
        if (name.Contains("通风口")) return "通风口";
        if (name.Contains("地砖")) return "地砖";
        if (name.Contains("楼梯")) return "楼梯";
        if (name.Contains("围栏")) return "围栏";
        if (name.Contains("墙")) return "墙体";
        return "其他模型";
    }

    private static void ConfigureStatic(GameObject model)
    {
        var interactions = model.GetComponentsInChildren<InteractableBaseA>(true);
        var dynamicParts = new HashSet<GameObject>();
        foreach (var interaction in interactions)
        {
            // The controller and its ancestors stay dynamic; fixed walls/frames can be static.
            for (var t = interaction.transform; t != model.transform.parent; t = t.parent)
                dynamicParts.Add(t.gameObject);
            var serialized = new SerializedObject(interaction);
            foreach (string property in new[] { "_doorLeaf", "_oppositeDoorLeaf", "triggerController", "billboardController" })
            {
                var reference = serialized.FindProperty(property)?.objectReferenceValue;
                var part = reference as GameObject;
                if (reference is Component component) part = component.gameObject;
                if (part == null) continue;
                foreach (var child in part.GetComponentsInChildren<Transform>(true))
                    dynamicParts.Add(child.gameObject);
            }
            if (!(interaction is DoorController))
                foreach (var child in interaction.GetComponentsInChildren<Transform>(true))
                    dynamicParts.Add(child.gameObject);
        }
        foreach (var child in model.GetComponentsInChildren<Transform>(true))
            SetStatic(child.gameObject, !dynamicParts.Contains(child.gameObject));
    }

    private static void SetStatic(GameObject go, bool value)
    {
        Undo.RecordObject(go, "Set model static flags");
        GameObjectUtility.SetStaticEditorFlags(go, value ? (StaticEditorFlags)(-1) : 0);
        if (PrefabUtility.IsPartOfPrefabInstance(go))
            PrefabUtility.RecordPrefabInstancePropertyModifications(go);
    }

    private static GameObject CreateGroup(string name, Transform parent, Scene scene)
    {
        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        Undo.RegisterCreatedObjectUndo(go, "Create model group");
        go.transform.SetParent(parent, false);
        return go;
    }

    // Entry point for Unity -batchmode -executeMethod.
    public static void OrganizeAndSave()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Organize();
        if (!EditorSceneManager.SaveScene(SceneManager.GetActiveScene()))
            throw new InvalidOperationException("Failed to save Test scene.");
    }
}
