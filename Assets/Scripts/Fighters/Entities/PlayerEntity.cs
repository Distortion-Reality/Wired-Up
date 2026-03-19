using Fusion;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(NetworkRigidbody))]
public class PlayerEntity : FighterEntity
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 dir;
        public Quaternion rotation;
    }

    [Networked] public NetworkString<_16> PlayerName { get; private set; }
    [Networked] public CharacterColor Character { get; private set; }

    public Player Player => fighter as Player;
    public PlayerToken PlayerToken { get => Token as PlayerToken; set => Token = value; }

    public override void Spawned()
    {
        NetworkManager.Instance.RegisterPlayer(Object.InputAuthority, this);

        base.Spawned();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        NetworkManager.Instance.UnregisterPlayer(Object.InputAuthority);

        base.Despawned(runner, hasState);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (GetInput(out NetworkInputData data))
        {
            Player.Move(data.dir);
            Player.Rotate(data.rotation);
        }

        base.FixedUpdateNetwork();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetSpawnPosition(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_UnwrapAttachedToken(NetworkString<_16> name, CharacterColor character)
    {
        PlayerName = name;
        Character = character;
    }

    public override void UnwrapAttachedToken()
    {
        base.UnwrapAttachedToken();

        if (Object.HasInputAuthority)
        {
            RPC_UnwrapAttachedToken(PlayerToken.name, PlayerToken.character);
        }
    }
}
