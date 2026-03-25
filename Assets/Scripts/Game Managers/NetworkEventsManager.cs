using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkEvents))]
public class NetworkEventsManager : SimulationBehaviour
{
    NetworkEvents events;

    // Start is called before the first frame update
    void Start()
    {
        Runner = NetworkRunnerManager.Instance.Runner;
        events = GetComponent<NetworkEvents>();

        Runner.AddCallbacks(events);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Runner)
        {
            Runner.RemoveCallbacks(events);
        }
    }
}
