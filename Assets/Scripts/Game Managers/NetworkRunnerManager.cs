using Fusion;
using UnityEngine;

public class NetworkRunnerManager : MonoBehaviour
{
    public static NetworkRunnerManager Instance { get; private set; }

    public NetworkRunner Runner { get; private set; }

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
