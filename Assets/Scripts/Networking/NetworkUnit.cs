using FlaxEngine;
using FlaxEngine.Networking;

public class NetworkUnit : Script
{
    public UnitBase Unit;
    public bool IsLocal = false;
    public int NetworkID;

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private float _interpolationSpeed = 5f;

    public override void OnStart()
    {
        if (Unit == null)
        {
            Unit = Actor.GetScript<UnitBase>();
        }

        NetworkID = Actor.GetHashCode();
    }

    public override void OnUpdate()
    {
        if (Unit == null || IsLocal) return;

        // Smoothly interpolate to target position/rotation
        if (Vector3.Distance(Unit.Position, _targetPosition) > 0.1f)
        {
            Unit.Position = Vector3.Lerp(
                Unit.Position,
                _targetPosition,
                _interpolationSpeed * Time.DeltaTime
            );
        }

        if (Quaternion.Angle(Unit.Rotation, _targetRotation) > 1f)
        {
            Unit.Rotation = Quaternion.Slerp(
                Unit.Rotation,
                _targetRotation,
                _interpolationSpeed * Time.DeltaTime
            );
        }
    }

    public void UpdateFromNetwork(UnitStatePacket packet)
    {
        if (Unit == null) return;

        _targetPosition = packet.Position;
        _targetRotation = packet.Rotation;
        Unit.Health = packet.Health;

        // Update faction if changed
        if (Unit.Faction?.FactionType != (FactionType)packet.Faction)
        {
            Unit.Faction = GameManager.Instance.GetFaction((FactionType)packet.Faction);
        }
    }

    public UnitStatePacket GetNetworkState()
    {
        return new UnitStatePacket
        {
            UnitID = NetworkID,
            Position = Unit.Position,
            Rotation = Unit.Rotation,
            Health = Unit.Health,
            Faction = (byte)(Unit.Faction?.FactionType ?? FactionType.None)
        };
    }

    [NetworkRpc]
    public void SendMovementCommand(Vector3 position)
    {
        if (IsLocal) return;
        Unit.MoveTo(position);
    }

    [NetworkRpc]
    public void SendAttackCommand(int targetID)
    {
        if (IsLocal) return;
        var target = GameManager.Instance.GetUnitByID(targetID);
        if (target != null)
        {
            Unit.Attack(target);
        }
    }
}