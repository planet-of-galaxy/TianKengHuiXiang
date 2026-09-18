using System;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>Shares the role instance lifetime and notifies systems on destruction.</summary>
public class RoleContext : MonoBehaviour
{
    [SerializeField] private int roleRuntimeIndex = -1;
    [SerializeField] private Transform weaponPosition;
    [SerializeField] private CinemachineCamera firstViewCinema;
    [SerializeField] private CinemachineCamera observeCinema;

    public event Action<RoleContext> OnDestroyed;

    public int RoleRuntimeIndex => roleRuntimeIndex;
    public Transform WeaponPosition => weaponPosition;
    public CinemachineCamera FirstViewCinema => firstViewCinema;
    public CinemachineCamera ObserveCinema => observeCinema;

    private void OnDestroy()
    {
        var onDestroyed = OnDestroyed;
        OnDestroyed = null;
        onDestroyed?.Invoke(this);
    }

    public void Initialize(int runtimeIndex)
    {
        roleRuntimeIndex = runtimeIndex;
    }
}
