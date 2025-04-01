using FlaxEngine;

public class UnitMovement : Script
{
    [Header("Movement Settings")]
    public float RotationSpeed = 10f;
    public float StoppingDistance = 1f;
    public float PathUpdateInterval = 0.5f;

    private UnitBase _unit;
    private Vector3 _destination;
    private float _lastPathUpdateTime;
    private bool _isMoving;

    public void Initialize(UnitBase unit)
    {
        _unit = unit;
    }

    public void SetDestination(Vector3 position)
    {
        _destination = position;
        _isMoving = true;
        _lastPathUpdateTime = Time.GameTime;
    }

    public void Stop()
    {
        _isMoving = false;
    }

    public void UpdateMovement()
    {
        if (!_isMoving) return;

        // Update path at intervals
        if (Time.GameTime - _lastPathUpdateTime > PathUpdateInterval)
        {
            _lastPathUpdateTime = Time.GameTime;
            // TODO: Implement pathfinding
        }

        // Simple direct movement (will replace with pathfinding)
        var direction = _destination - Actor.Position;
        if (direction.Length < StoppingDistance)
        {
            _isMoving = false;
            return;
        }

        direction.Normalize();
        Actor.Position += direction * _unit.MoveSpeed * Time.DeltaTime;

        // Smooth rotation
        var targetRotation = Quaternion.LookRotation(direction, Vector3.Up);
        Actor.Orientation = Quaternion.Slerp(
            Actor.Orientation,
            targetRotation,
            RotationSpeed * Time.DeltaTime
        );
    }

    public bool ReachedDestination => 
        !_isMoving || Vector3.Distance(Actor.Position, _destination) <= StoppingDistance;
}