using FlaxEngine;

public class UnitCombat : Script
{
    [Header("Combat Settings")]
    public float AttackCooldown = 1f;
    public float CriticalChance = 0.05f;
    public float CriticalMultiplier = 2f;

    private UnitBase _unit;
    private UnitBase _target;
    private float _lastAttackTime;
    private bool _isAttacking;

    public void Initialize(UnitBase unit)
    {
        _unit = unit;
    }

    public void EngageTarget(UnitBase target)
    {
        _target = target;
        _isAttacking = true;
    }

    public void StopAttack()
    {
        _isAttacking = false;
        _target = null;
    }

    public void UpdateCombat()
    {
        if (!_isAttacking || _target == null) return;

        // Check if target is in range
        float distance = Vector3.Distance(_unit.Actor.Position, _target.Actor.Position);
        if (distance > _unit.AttackRange)
        {
            // Move toward target if out of range
            _unit.MoveTo(_target.Actor.Position);
            return;
        }

        // Face target
        var direction = (_target.Actor.Position - _unit.Actor.Position).Normalized;
        var targetRotation = Quaternion.LookRotation(direction, Vector3.Up);
        _unit.Actor.Orientation = Quaternion.Slerp(
            _unit.Actor.Orientation,
            targetRotation,
            _unit.Movement.RotationSpeed * Time.DeltaTime
        );

        // Attack if cooldown is ready
        if (Time.GameTime - _lastAttackTime > AttackCooldown)
        {
            PerformAttack();
            _lastAttackTime = Time.GameTime;
        }
    }

    private void PerformAttack()
    {
        if (_target == null) return;

        // Calculate damage with critical chance
        float damage = _unit.AttackDamage;
        if (Random.Range(0f, 1f) < CriticalChance)
        {
            damage *= CriticalMultiplier;
            // TODO: Play critical hit effect
        }

        _target.TakeDamage(damage);

        // TODO: Play attack animation and sound
    }

    public bool HasValidTarget => _isAttacking && _target != null && !_target.IsDead;
}