using FlaxEngine;

public class UnitBase : Script
{
    [Header("Unit Properties")]
    public float Health = 100f;
    public float MoveSpeed = 5f;
    public float AttackRange = 10f;
    public float AttackDamage = 10f;
    public float Armor = 0.1f; // 10% damage reduction

    [Header("References")] 
    public FactionBase Faction;
    public UnitMovement Movement;
    public UnitCombat Combat;
    public UnitVisuals Visuals;

    private Vector3 _targetPosition;
    private UnitBase _attackTarget;

    public override void OnStart()
    {
        // Initialize components
        Movement = Actor.GetOrAddChild<UnitMovement>();
        Combat = Actor.GetOrAddChild<UnitCombat>();
        Visuals = Actor.GetOrAddChild<UnitVisuals>();
        
        Movement.Initialize(this);
        Combat.Initialize(this);
        Visuals.Initialize(this);
    }

    public override void OnUpdate()
    {
        if (Health <= 0)
        {
            Destroy();
            return;
        }

        Movement.UpdateMovement();
        Combat.UpdateCombat();
        Visuals.UpdateVisuals();
    }

    public void MoveTo(Vector3 position)
    {
        _targetPosition = position;
        Movement.SetDestination(position);
    }

    public void Attack(UnitBase target)
    {
        _attackTarget = target;
        Combat.EngageTarget(target);
    }

    public void TakeDamage(float amount)
    {
        Health -= amount * (1 - Armor);
        Visuals.PlayHitEffect();
    }

    public void Destroy()
    {
        Faction?.Units.Remove(this);
        Actor.Destroy();
    }
}