using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

public class LobbyPlayer : EntityBehaviour<ILobbyPlayerState>
{
    List<GameObject> models = new List<GameObject>();
    int currentIndex = 0;
    public int CurrentCharacter { get => currentIndex; }
    bool ready = false;
    public bool IsReady { get => ready; }

    LobbyManager lobbyManager;
    TMPro.TextMeshProUGUI playerName;
    Button nextCharacterButton;
    Button previousCharacterButton;
    Button readyButton;
    public Button ReadyButton { get => readyButton; }

    public override void Attached()
    {
        CreateCharactersModels();

        lobbyManager = GameObject.FindObjectOfType<LobbyManager>();
        playerName = GetComponentInChildren<TMPro.TextMeshProUGUI>();

        if (entity.IsOwner)
        {
            nextCharacterButton = GameObject.Find("NextCharacterButton").GetComponent<Button>();
            nextCharacterButton.onClick.AddListener(NextCharacter);

            previousCharacterButton = GameObject.Find("PreviousCharacterButton").GetComponent<Button>();
            previousCharacterButton.onClick.AddListener(PreviousCharacter);

            Button leaveButton = GameObject.Find("LeaveButton").GetComponent<Button>();
            leaveButton.onClick.AddListener(() => lobbyManager.ReturnToMenu());

            readyButton = GameObject.Find("ReadyButton").GetComponent<Button>();
            readyButton.onClick.AddListener(() => state.ready = !ready);

            state.name = PlayerPrefs.GetString(Menu.PlayerNameKey);
            state.character = 0;
            state.ready = false;
        }

        state.AddCallback("name", NameChanged);
        state.AddCallback("character", CharacterChanged);
        state.AddCallback("ready", ReadyChanged);

        NameChanged();
        models[currentIndex].SetActive(true);
    }

    void CreateCharactersModels()
    {
        Transform characterModel = gameObject.transform.Find("Character/CharacterModel");
        CharacterManager characterManager = GameObject.FindObjectOfType<CharacterManager>();

        foreach (var color in characterManager.GetCharacterColors())
        {
            GameObject model = Instantiate(characterManager.GetPrefab(color), parent: characterModel);
            model.SetActive(false);
            models.Add(model);
        }
    }

    void NextCharacter()
    {
        int index = currentIndex + 1;
        if (index == models.Count)
        {
            index = 0;
        }
        state.character = index; 
    }

    void PreviousCharacter()
    {
        int index = currentIndex - 1;
        if (index == -1) {
            index = models.Count - 1;
        }
        state.character = index;
    }

    void CharacterChanged()
    {
        models[currentIndex].SetActive(false);
        currentIndex = state.character;
        models[currentIndex].SetActive(true);

        lobbyManager.CheckOwnerCharacterAvailable();
    }

     void NameChanged()
    {
        playerName.text = state.name;
    }

    void ReadyChanged()
    {
        ready = state.ready;

        playerName.color = ready ? Color.green : Color.white;

        if (entity.IsOwner)
        {
            nextCharacterButton.interactable = !ready;
            previousCharacterButton.interactable = !ready;
        }
        else
        {
            lobbyManager.CheckOwnerCharacterAvailable();
        }

        if (BoltNetwork.IsServer && lobbyManager.CanStart())
        {
            BoltNetwork.LoadScene("Level2Scene");
            //LobbyStartEvent.Create().Send();
        }
    }
}
