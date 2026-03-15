using Fusion;
using UnityEngine;

public class PlayerEntity : FighterEntity
{
    [Networked] public NetworkString<_16> PlayerName { get; private set; }
    [Networked] public CharacterColor Character { get; private set; }

    Player Player => fighter as Player;
    public PlayerToken PlayerToken { get => Token as PlayerToken; set => Token = value; }

    public override void Spawned()
    {
        NetworkManager.Instance.IncrementAllyCount();

        base.Spawned();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_WireConnect()
    {
        Player.Wire.Connect();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_WireStayConnected(NetworkId targetId)
    {
        Fighter target = Runner.FindObject(targetId).GetComponent<Fighter>();

        Player.Target = target;
        Player.Wire.StayConnected();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_WireDisconnect()
    {
        Player.Wire.Disconnect();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ChangeWireRotation(Vector3 lookAt, bool reset)
    {
        Transform wire = Player.Wire.transform.parent;
        if (reset)
            wire.transform.localRotation = Quaternion.identity;
        else
            wire.transform.LookAt(lookAt);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_GameLose(string playerName, CharacterColor character)
    {
        NetworkManager.Instance.GameLose(playerName, character);
    }

    public override void UnwrapAttachedToken()
    {
        base.UnwrapAttachedToken();

        PlayerName = PlayerToken.name;
        Character = PlayerToken.character;
    }
}
