using QFramework;

public interface IWeaponConfigProvider : IUtility
{
    WeaponConfig GetWeaponConfig(int weaponId);
    bool HasWeapon(int weaponId);
    int GetWeaponCount();
}
