using QFramework;
using UnityEngine;
using UnityEngine.AI;

/// <summary>Chases the controlled player and resolves melee attacks through the damage system.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(MonsterContext), typeof(NavMeshAgent))]
public class MonsterChaseController : MonoBehaviour, IController
{
    [SerializeField, Min(0.1f)] private float attackRange = 1.6f;
    [SerializeField, Min(0f)] private float attackDamage = 5f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 1.5f;
    [SerializeField, Min(0f)] private float attackWindup = 0.4f;
    private MonsterContext context;
    private NavMeshAgent agent;
    private IRoleInstanceSystem roles;
    private IGamePauseSystem pause;
    private RoleContext attackTarget;
    private float cooldown;
    private float windup;
    private bool attacking;
    private float nextPathUpdate;

    public IArchitecture GetArchitecture() => TianArchitecture.Interface;

    private void Awake()
    {
        context = GetComponent<MonsterContext>();
        agent = GetComponent<NavMeshAgent>();
        roles = this.GetSystem<IRoleInstanceSystem>();
        pause = this.GetSystem<IGamePauseSystem>();
        var config = this.GetUtility<IMonsterConfigProvider>().GetMonster(context.ConfigId);
        agent.speed = config != null ? Mathf.Max(0f, config.moveSpeed) : 2f;
        agent.stoppingDistance = attackRange * 0.8f;
        if (context.MonsterAnimatorController != null)
            context.MonsterAnimatorController.OnAttackHitFrame += Hit;
    }

    private void Update()
    {
        if (pause.IsPaused || !context.MonsterStateMachine.StateMachine.IsInState<MonsterDefualtState>())
        {
            Stop();
            // Pausing freezes the attack; stun/death cancels it.
            if (!pause.IsPaused) CancelAttack();
            return;
        }

        cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);
        var target = roles.ControllingRole;
        if (!IsAlive(target))
        {
            Stop();
            CancelAttack();
            return;
        }
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var spawn, 2f, agent.areaMask))
                agent.Warp(spawn.position);
            else return;
        }
        if (attacking)
        {
            Stop();
            Face(attackTarget != null ? attackTarget.transform.position : target.transform.position);
            windup -= Time.deltaTime;
            if (windup <= 0f) Hit();
            return;
        }
        var offset = target.transform.position - transform.position;
        if (offset.sqrMagnitude <= attackRange * attackRange && HasLineOfSight(target))
        {
            Stop();
            Face(target.transform.position);
            if (cooldown <= 0f)
            {
                attackTarget = target;
                attacking = true;
                windup = attackWindup;
                cooldown = Mathf.Max(attackCooldown, attackWindup + 0.1f);
                context.MonsterAnimatorController?.StartAttack(AttackType.Light);
            }
            return;
        }
        agent.isStopped = false;
        if (Time.time >= nextPathUpdate)
        {
            nextPathUpdate = Time.time + 0.2f;
            if (NavMesh.SamplePosition(target.transform.position, out var hit, 1.5f, agent.areaMask))
                agent.SetDestination(hit.position);
            else agent.ResetPath();
        }
        var localVelocity = transform.InverseTransformDirection(agent.velocity);
        context.MonsterAnimatorController?.PlayMoveAnimation(localVelocity.z / Mathf.Max(agent.speed, 0.01f),
            localVelocity.x / Mathf.Max(agent.speed, 0.01f));
    }

    private bool IsAlive(RoleContext target)
    {
        return target != null && target.isActiveAndEnabled
            && this.GetModel<IRoleRuntimeModel>().TryGetRoleRuntime(target.RoleRuntimeIndex, out var runtime)
            && runtime.CurHealth.Value > 0f;
    }

    private bool HasLineOfSight(RoleContext target)
    {
        var start = transform.position + Vector3.up;
        var end = target.transform.position + Vector3.up;
        foreach (var hit in Physics.RaycastAll(start, end - start, Vector3.Distance(start, end),
                     Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.IsChildOf(transform) || hit.transform.IsChildOf(target.transform)) continue;
            return false;
        }
        return true;
    }

    private void Hit()
    {
        if (!attacking || pause.IsPaused) return;
        if (!context.MonsterStateMachine.StateMachine.IsInState<MonsterDefualtState>())
        {
            CancelAttack();
            return;
        }
        var target = attackTarget;
        CancelAttack();
        if (!IsAlive(target) || (target.transform.position - transform.position).sqrMagnitude > attackRange * attackRange
            || !HasLineOfSight(target)) return;
        var receiver = target.GetComponent<IGetDamage>();
        if (receiver != null && !receiver.IsDisabled)
            receiver.GetDamage(new DamageInfo { type = DamageType.Melee, hintCount = 1, hintValue = attackDamage });
    }

    private void Face(Vector3 position)
    {
        var direction = position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 720f * Time.deltaTime);
    }

    private void Stop()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
        context.MonsterAnimatorController?.PlayMoveAnimation(0f, 0f);
    }

    private void CancelAttack()
    {
        attacking = false;
        attackTarget = null;
    }

    private void OnDisable()
    {
        Stop();
        CancelAttack();
    }

    private void OnDestroy()
    {
        if (context != null && context.MonsterAnimatorController != null)
            context.MonsterAnimatorController.OnAttackHitFrame -= Hit;
    }
}
