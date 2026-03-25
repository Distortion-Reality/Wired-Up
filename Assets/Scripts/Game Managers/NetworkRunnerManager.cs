using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkRunner))]
public class NetworkRunnerManager : SimulationBehaviour
{
    public static NetworkRunnerManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Runner = GetComponent<NetworkRunner>();
    }
}
