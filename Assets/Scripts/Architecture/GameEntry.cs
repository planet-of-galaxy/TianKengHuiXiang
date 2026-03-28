using QFramework;
using UnityEngine;

public class GameEntry
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init()
    {
        ResKit.Init();
        UIKit.Root.CanvasScaler.matchWidthOrHeight = 1f;
        UIKit.Root.CanvasScaler.referenceResolution = new Vector2(1980f, 1080f);
        UIKit.OpenPanel<StartPanel>(prefabName: "resources://UI/Panel/startpanel");
    }
}
