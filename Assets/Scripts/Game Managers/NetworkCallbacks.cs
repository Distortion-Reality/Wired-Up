using UnityEngine;
using Photon.Bolt;

[BoltGlobalBehaviour("Level2Scene")]
public class NetworkCallbacks : GlobalEventListener {

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        Transform players = GameObject.Find("Players").transform;
        BoltEntity player = BoltNetwork.Instantiate(BoltPrefabs.Player, players.position, players.rotation);

        GameObject playerCamera = GameObject.Find("PlayerCamera");
        Cinemachine.CinemachineFreeLook cinemachine = playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>();
        cinemachine.Follow = player.transform;
        cinemachine.LookAt = player.transform.Find("CameraLookTarget");
    }
}
