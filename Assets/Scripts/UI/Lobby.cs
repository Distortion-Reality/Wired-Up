using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Bolt;

public class Lobby : GlobalEventListener
{
    private List<GameObject> models = new List<GameObject>();
    private int selectedIndex = 0;

    private void CreateCharactersModels()
    {
        GameObject characterModel = GameObject.Find("PlayerCharacter/CharacterModel");
        CharacterManager characterManager = GameObject.FindObjectOfType<CharacterManager>();

        foreach (var color in characterManager.GetCharacterColors())
        {
            GameObject model = Instantiate(characterManager.GetPrefab(color), parent: characterModel.transform);
            model.SetActive(false);
            models.Add(model);
        }
    }

    void Start() 
    {
        CreateCharactersModels();
        SelectCurrentCharacter(true);
    }

    public void Ready()
    {

    }

    public void Leave()
    {
        BoltLauncher.Shutdown();
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    private void SelectCurrentCharacter(bool active)
    {
        models[selectedIndex].SetActive(active);
    }

    private void CheckAvailableCharacter()
    {

    }

    public void NextCharacter()
    {
        SelectCurrentCharacter(false);
        selectedIndex++;
        if (selectedIndex == models.Count)
        {
            selectedIndex = 0;
        }
        SelectCurrentCharacter(true);
    }

    public void PreviousCharacter()
    {
        SelectCurrentCharacter(false);
        selectedIndex--;
        if (selectedIndex == -1) {
            selectedIndex = models.Count - 1;
        }
        SelectCurrentCharacter(true);
    }

    public override void Disconnected(BoltConnection connection)
    {
        if (BoltNetwork.Server.Equals(connection))
        {
            Leave();
        }
    }
}
