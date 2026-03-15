using Fusion;

public class BossEntity : EnemyEntity
{
    [Networked] public float Speed { get; set; }
    [Networked] public float Direction { get; set; }
    [Networked] public bool OnGround { get; set; }

    Boss Boss => Enemy as Boss;

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_AttackShoot()
    {
        Boss.AttackShoot();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_GameWin()
    {
        NetworkManager.Instance.GameWin();
    }
}
