using System.Collections.Generic;
using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    public List<GameObject> particlesPrefabs;
    readonly Dictionary<ParticlesId, GameObject> particles = new Dictionary<ParticlesId, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < particlesPrefabs.Count; i++)
            particles.Add((ParticlesId)i, particlesPrefabs[i]);
    }

    public GameObject GetParticlesPrefab(ParticlesId particlesId)
    {
        return particles[particlesId];
    }

    public Vector3 GetParticlesPosition(ParticlesId particlesId, Fighter target)
    {
        Vector3 position;
        switch (particlesId)
        {
            case ParticlesId.EnemyDamage:
            case ParticlesId.PlayerDamage:
            case ParticlesId.KirinDamage:
            case ParticlesId.KirinBurst:
            case ParticlesId.Heal:
            case ParticlesId.Death:
                position = target.CentrePosition;
                break;
            case ParticlesId.Buff:
            case ParticlesId.Debuff:
            case ParticlesId.Stun:
                position = target.TopPosition;
                break;
            case ParticlesId.Target:
                position = target.BottomPosition;
                break;
            default:
                position = Vector3.zero;
                break;
        }

        return position;
    }

    public bool CanParticlesAfterDeath(ParticlesId particlesId)
    {
        return particlesId == ParticlesId.EnemyDamage || particlesId == ParticlesId.PlayerDamage ||
            particlesId == ParticlesId.KirinDamage || particlesId == ParticlesId.Death;
    }
}
