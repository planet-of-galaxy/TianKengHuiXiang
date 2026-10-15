using QFramework;
using UnityEngine;

/// <summary>在动画命中帧执行近战伤害判定。</summary>
public class MeleeWeaponAttack : WeaponAttackBase
{
    [SerializeField] private Transform _damagePoint;

    private float _attackCooldown;
    private bool _isAttacking;
    private bool _hasDetectedTarget;
    private float _attackRange;
    private int _monsterMask;
    private DamageInfo _damageInfo;

    private void Update()
    {
        _attackCooldown = Mathf.Max(0f, _attackCooldown - Time.deltaTime);
    }

    public override void StartAttack(AttackType attackType)
    {
        if (!isActiveAndEnabled || _isAttacking || _attackCooldown > 0f)
            return;

        if (_damagePoint == null)
        {
            Debug.LogWarning("MeleeWeaponAttack 未设置伤害检测点。", this);
            return;
        }

        int weaponConfigId = weaponContext.WeaponConfigId;
        var weaponConfig = TianArchitecture.Interface.GetUtility<IWeaponConfigProvider>()
            .GetWeaponConfig(weaponConfigId);
        var config = attackType == AttackType.Light ? weaponConfig?.lightAttack : weaponConfig?.heavyAttack;
        if (config == null)
        {
            Debug.LogWarning($"MeleeWeaponAttack 找不到武器 {weaponConfigId} 的攻击配置。", this);
            return;
        }

        _monsterMask = LayerMask.GetMask("Monster");
        if (_monsterMask == 0)
        {
            Debug.LogWarning("MeleeWeaponAttack 找不到 Monster 层。", this);
            return;
        }

        bool isCritical = Random.value < Mathf.Clamp01(config.criticalRate);
        _damageInfo = new DamageInfo
        {
            type = config.damageType,
            hintCount = 1,
            hintValue = config.attackPower * (isCritical ? config.criticalMultiplier : 1f)
        };
        _attackRange = Mathf.Max(0f, config.attackRange);
        _attackCooldown = Mathf.Max(0f, config.attackSpeed);
        _hasDetectedTarget = false;
        _isAttacking = true;
    }

    public override void DoAttackHitFrame()
    {
        if (!isActiveAndEnabled || !_isAttacking || _hasDetectedTarget || _damagePoint == null)
            return;

        var targets = Physics.OverlapSphere(_damagePoint.position, _attackRange,
            _monsterMask, QueryTriggerInteraction.Collide);
        if (targets.Length == 0) return;

        // 先锁定本次判定，防止后续命中帧或伤害回调重复触发。
        _hasDetectedTarget = true;
        foreach (var target in targets)
        {
            if (TryDeliverDamage(target.gameObject, _damageInfo))
                break;
        }
    }

    public override void DoAttackInterrupt()
    {
        EndAttack();
    }

    public override void EndAttack()
    {
        _isAttacking = false;
        _hasDetectedTarget = false;
        _damageInfo = default;
    }

    private void OnDisable()
    {
        DoAttackInterrupt();
    }
}
