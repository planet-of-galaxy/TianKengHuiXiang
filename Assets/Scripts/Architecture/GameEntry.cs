using QFramework;
using UnityEngine;

public class GameEntry
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init()
    {
        UIKit.OpenPanel<StartPanel>();
    }
}
