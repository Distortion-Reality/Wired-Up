using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Bolt;

[BoltGlobalBehaviour("Level2Scene")]
public class NetworkCallbacks : GlobalEventListener {

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
        // Spawn player
        CharacterManager characterManager = GameObject.FindObjectOfType<CharacterManager>();
        PlayerInfo info = new PlayerInfo();
        info.guid = Guid.NewGuid();
        info.name = PlayerPrefs.GetString(PlayerPrefKey.PlayerName);
        info.character = characterManager.CurrentCharacter;
        Transform players = GameObject.Find("Players").transform;
        BoltEntity entity = BoltNetwork.Instantiate(BoltPrefabs.Player, info, players.position, players.rotation);

        // Setup player camera
        GameObject playerCamera = GameObject.Find("PlayerCamera");
        Cinemachine.CinemachineFreeLook cinemachine = playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>();
        cinemachine.Follow = entity.transform;
        cinemachine.LookAt = entity.transform.Find("CameraLookTarget");
    }

    public override void Disconnected(BoltConnection connection)
    {
        BoltLauncher.Shutdown();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
