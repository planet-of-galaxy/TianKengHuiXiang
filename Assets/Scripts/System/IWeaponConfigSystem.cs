using QFramework;

public interface IWeaponConfigSystem : ISystem
{
    WeaponConfig GetWeaponConfig(int weaponId);
}
