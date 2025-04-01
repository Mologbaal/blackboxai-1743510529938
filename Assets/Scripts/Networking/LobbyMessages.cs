using FlaxEngine.Networking;

[NetworkSerializable]
public struct PlayerJoinMessage : INetworkSerializable
{
    public string PlayerName;
    public bool IsHost;

    public void Serialize(ref NetworkMessage message)
    {
        message.WriteString(PlayerName);
        message.WriteBool(IsHost);
    }

    public void Deserialize(ref NetworkMessage message)
    {
        PlayerName = message.ReadString();
        IsHost = message.ReadBool();
    }
}

[NetworkSerializable]
public struct PlayerReadyMessage : INetworkSerializable
{
    public bool IsReady;

    public void Serialize(ref NetworkMessage message)
    {
        message.WriteBool(IsReady);
    }

    public void Deserialize(ref NetworkMessage message)
    {
        IsReady = message.ReadBool();
    }
}

[NetworkSerializable]
public struct PlayerFactionMessage : INetworkSerializable
{
    public string PlayerName;
    public byte Faction;

    public void Serialize(ref NetworkMessage message)
    {
        message.WriteString(PlayerName);
        message.WriteByte(Faction);
    }

    public void Deserialize(ref NetworkMessage message)
    {
        PlayerName = message.ReadString();
        Faction = message.ReadByte();
    }
}

[NetworkSerializable]
public struct StartGameMessage : INetworkSerializable
{
    public byte Faction;

    public void Serialize(ref NetworkMessage message)
    {
        message.WriteByte(Faction);
    }

    public void Deserialize(ref NetworkMessage message)
    {
        Faction = message.ReadByte();
    }
}