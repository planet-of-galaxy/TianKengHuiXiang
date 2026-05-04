using QFramework;
using UnityEngine;

public class PlayerSystem : AbstractSystem, IPlayerSystem
{
    private GameObject _currentPlayer;
    private PlayerDataModel _playerModel;
    private IRoleSystem _roleSystem;

    protected override void OnInit()
    {
        _playerModel = this.GetModel<PlayerDataModel>();
        _roleSystem = this.GetSystem<IRoleSystem>();
    }

    public void InitPlayer(int roleId)
    {
        var roleConfig = _roleSystem.GetRoleConfig(roleId);
        if (roleConfig == null)
        {
            Debug.LogError($"[PlayerSystem] RoleConfig not found for roleId: {roleId}");
            return;
        }

        _playerModel.MaxInfo.Value = new PlayerInfo { moveSpeed = roleConfig.moveSpeed, health = roleConfig.health };
        _playerModel.CurInfo.Value = new PlayerInfo { moveSpeed = roleConfig.moveSpeed, health = roleConfig.health };
        _playerModel.RoleId.Value = roleId;
    }

    public void CreatePlayer(Transform trans)
    {
        if (_currentPlayer != null)
        {
            Debug.LogWarning("[PlayerSystem] Player already exists, destroying previous player");
            DestroyPlayer();
        }

        var roleId = _playerModel.RoleId.Value;
        var roleConfig = _roleSystem.GetRoleConfig(roleId);
        if (roleConfig == null)
        {
            Debug.LogError($"[PlayerSystem] RoleConfig not found for roleId: {roleId}");
            return;
        }

        var prefab = Resources.Load<GameObject>($"Prefabe/Role/{roleConfig.name}");
        if (prefab == null)
        {
            Debug.LogError($"[PlayerSystem] Prefab not found: Prefabe/Role/{roleConfig.name}");
            return;
        }

        _currentPlayer = Object.Instantiate(prefab, trans.position, trans.rotation);
    }

    public void DestroyPlayer()
    {
        if (_currentPlayer != null)
        {
            Object.Destroy(_currentPlayer);
            _currentPlayer = null;
        }

        _playerModel.RoleId.Value = -1;
        _playerModel.MaxInfo.Value = new PlayerInfo();
        _playerModel.CurInfo.Value = new PlayerInfo();
    }

    public void Teleport(Transform trans)
    {
        if (_currentPlayer == null)
        {
            Debug.LogWarning("[PlayerSystem] No player to teleport");
            return;
        }

        _currentPlayer.transform.position = trans.position;
        _currentPlayer.transform.rotation = trans.rotation;
    }
}
