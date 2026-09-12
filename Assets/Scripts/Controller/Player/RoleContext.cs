using Unity.Cinemachine;
using UnityEngine;

public class RoleContext : MonoBehaviour
{
    [SerializeField] private int roleRuntimeIndex = -1;
    [SerializeField] private Transform weaponPosition;
    [SerializeField] private CinemachineCamera firstViewCinema;
    [SerializeField] private CinemachineCamera observeCinema;

    public int RoleRuntimeIndex => roleRuntimeIndex;
    public Transform WeaponPosition => weaponPosition;
    public CinemachineCamera FirstViewCinema => firstViewCinema;
    public CinemachineCamera ObserveCinema => observeCinema;

    public void Initialize(int runtimeIndex)
    {
        roleRuntimeIndex = runtimeIndex;
    }
}
