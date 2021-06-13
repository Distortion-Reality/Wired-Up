using UnityEngine;
using Photon.Bolt;

[BoltGlobalBehaviour("Level2Scene")]
public class NetworkCallbacks : GlobalEventListener {

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        Vector3 spawnPosition = Vector3.zero;
        BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPosition, Quaternion.identity);

        GameObject playerCamera = GameObject.Find("PlayerCamera");
        Cinemachine.CinemachineFreeLook cinemachine = playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>();
        cinemachine.Follow = entity.transform;
        cinemachine.LookAt = entity.transform.Find("CameraLookTarget");
    }
}
