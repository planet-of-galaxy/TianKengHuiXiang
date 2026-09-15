using UnityEngine;

public class WeaponContext : MonoBehaviour
{
    [SerializeField] private int weaponConfigId;

    public int WeaponConfigId => weaponConfigId;
}
