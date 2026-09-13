using System.Collections;
using QFramework;
using UnityEngine;

/// <summary>使用武器配置执行近战轻攻击和重攻击。</summary>
public class MeleeWeaponAttack : WeaponAttackBase
{
    private const float DamageCheckInterval = 0.03f;

    [SerializeField] private Transform _damagePoint;

    private float _attackCooldown;
    private bool _isAttacking;

    private void Update()
    {
        _attackCooldown = Mathf.Max(0f, _attackCooldown - Time.deltaTime);
    }

    // 返回本次攻击预先计算的伤害；实际伤害在前摇后命中时交付，未能发起攻击则返回 default。
    public override DamageInfo ILightAttack()
    {
        return TryStartAttack(false);
    }

    public override DamageInfo IHeavyAttack()
    {
        return TryStartAttack(true);
    }

    private DamageInfo TryStartAttack(bool heavyAttack)
    {
        if (!isActiveAndEnabled || _isAttacking || _attackCooldown > 0f)
        {
            return default;
        }

        if (_damagePoint == null)
        {
            Debug.LogWarning("MeleeWeaponAttack 未设置伤害检测点。", this);
            return default;
        }

        var weaponConfig = TianArchitecture.Interface.GetUtility<IWeaponConfigProvider>()
            .GetWeaponConfig(weaponConfigId);
        var attackConfig = heavyAttack ? weaponConfig?.heavyAttack : weaponConfig?.lightAttack;
        if (attackConfig == null)
        {
            Debug.LogWarning($"MeleeWeaponAttack 找不到武器 {weaponConfigId} 的攻击配置。", this);
            return default;
        }

        int monsterMask = LayerMask.GetMask("Monster");
        if (monsterMask == 0)
        {
            Debug.LogWarning("MeleeWeaponAttack 找不到 Monster 层。", this);
            return default;
        }

        bool isCritical = Random.value < Mathf.Clamp01(attackConfig.criticalRate);
        var damageInfo = new DamageInfo
        {
            type = attackConfig.damageType,
            hintCount = 1,
            hintValue = attackConfig.attackPower * (isCritical ? attackConfig.criticalMultiplier : 1f)
        };

        _attackCooldown = Mathf.Max(0f, attackConfig.attackSpeed);
        _isAttacking = true;
        StartCoroutine(Attack(attackConfig, monsterMask, damageInfo));
        return damageInfo;
    }

    private IEnumerator Attack(WeaponAttackConfig config, int monsterMask, DamageInfo damageInfo)
    {
        try
        {
            if (config.attackWindup > 0f)
            {
                yield return new WaitForSeconds(config.attackWindup);
            }

            float endTime = Time.time + Mathf.Max(0f, config.attackDuration);
            var checkDelay = new WaitForSeconds(DamageCheckInterval);
            while (Time.time < endTime && _damagePoint != null)
            {
                var targets = Physics.OverlapSphere(_damagePoint.position,
                    Mathf.Max(0f, config.attackRange), monsterMask, QueryTriggerInteraction.Collide);
                foreach (var target in targets)
                {
                    if (TryDeliverDamage(target.gameObject, damageInfo))
                    {
                        yield break;
                    }
                }

                yield return checkDelay;
            }
        }
        finally
        {
            _isAttacking = false;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _isAttacking = false;
    }
}
