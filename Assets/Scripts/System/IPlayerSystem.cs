using QFramework;
using UnityEngine;

public interface IPlayerSystem : ISystem
{
    void InitPlayer(int roleId);
    void CreatePlayer(Transform trans);
    void DestroyPlayer();
    void Teleport(Transform trans);
}
