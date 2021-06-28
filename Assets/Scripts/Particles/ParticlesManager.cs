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

    public GameObject GetParticlePrefab(ParticlesId particlesId)
    {
        return particles[particlesId];
    }
}
