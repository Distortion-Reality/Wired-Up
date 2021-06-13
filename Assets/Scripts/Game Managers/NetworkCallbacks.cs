using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

[BoltGlobalBehaviour("Level1Scene")]
public class NetworkCallbacks : GlobalEventListener {

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        var spawnPosition = Vector3.zero;
        var entity = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPosition, Quaternion.identity);
        var playerCamera = GameObject.Find("PlayerCamera");
        var cinemachine = playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>();
        cinemachine.Follow = entity.transform;
        cinemachine.LookAt = entity.transform.Find("CameraLookTarget");
    }
}
