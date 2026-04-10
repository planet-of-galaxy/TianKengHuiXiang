using QFramework;
using UnityEngine;

public class TianArchitecture : Architecture<TianArchitecture>
{
    protected override void Init()
    {
        Debug.Log("Tian Keng architecture initializing.");
        // 注册Model

        // 注册System

        // 注册Storage

        Debug.Log("Tian Keng architecture initialized.");
    }
}
