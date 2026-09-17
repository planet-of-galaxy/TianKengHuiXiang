using UnityEngine;

public class WeaponContext : MonoBehaviour
{
    [SerializeField] private int weaponConfigId;
    private IAttack attack;

    public int WeaponConfigId => weaponConfigId;
    public IAttack Attack => attack ??= GetComponent<IAttack>();

    private void Awake()
    {
        attack = GetComponent<IAttack>();
    }
}
