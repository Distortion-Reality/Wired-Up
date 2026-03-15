using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
public class LobbyPlayer : NetworkBehaviour
{
    readonly List<GameObject> models = new List<GameObject>();

    [Networked(OnChanged = nameof(OnNameChanged))]
    public NetworkString<_16> PlayerName { get; private set; }

    [Networked(OnChanged = nameof(OnCharacterChanged))]
    public int CurrentCharacterIndex { get; private set; } = 0;

    [Networked(OnChanged = nameof(OnReadyChanged))]
    public bool IsReady { get; private set; } = false;

    TextMeshProUGUI playerNameText;
    Button nextCharacterButton;
    Button previousCharacterButton;
    Button readyButton;

    public Button ReadyButton { get => readyButton; }

    public override void Spawned()
    {
        playerNameText = GetComponentInChildren<TextMeshProUGUI>();

        LobbyManager.Instance.RegisterPlayer(this);

        CreateCharactersModels();

        if (Object.HasInputAuthority)
        {
            nextCharacterButton = GameObject.Find("NextCharacterButton").GetComponent<Button>();
            nextCharacterButton.onClick.AddListener(NextCharacter);

            previousCharacterButton = GameObject.Find("PreviousCharacterButton").GetComponent<Button>();
            previousCharacterButton.onClick.AddListener(PreviousCharacter);

            Button leaveButton = GameObject.Find("LeaveButton").GetComponent<Button>();
            leaveButton.onClick.AddListener(OnLeave);

            readyButton = GameObject.Find("ReadyButton").GetComponent<Button>();
            readyButton.onClick.AddListener(OnReady);

            RPC_SetPlayerName(PlayerPrefs.GetString(PlayerPrefKey.PlayerName));
            RPC_SetCharacter(0);
            RPC_SetReady(false);
        }

        models[CurrentCharacterIndex].SetActive(true);
    }

    void CreateCharactersModels()
    {
        Transform characterModel = gameObject.transform.Find("Character/CharacterModel");

        foreach (var color in CharacterColorHelper.Values())
        {
            GameObject model = Instantiate(CharacterManager.Instance.GetPrefab(color), parent: characterModel);
            model.SetActive(false);
            models.Add(model);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerName(string name)
    {
        PlayerName = name;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCharacter(int index)
    {
        CurrentCharacterIndex = index;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady(bool ready)
    {
        IsReady = ready;
    }

    static void OnNameChanged(Changed<LobbyPlayer> changed)
    {
        changed.Behaviour.NameChanged();
    }

    static void OnCharacterChanged(Changed<LobbyPlayer> changed)
    {
        changed.LoadOld();
        int oldIndex = changed.Behaviour.CurrentCharacterIndex;

        changed.LoadNew();
        int newIndex = changed.Behaviour.CurrentCharacterIndex;

        changed.Behaviour.CharacterChanged(oldIndex, newIndex);
    }

    static void OnReadyChanged(Changed<LobbyPlayer> changed)
    {
        changed.Behaviour.ReadyChanged();
    }

    void NameChanged()
    {
        playerNameText.text = PlayerName.Value;
    }

    void CharacterChanged(int oldIndex, int newIndex)
    {
        models[oldIndex].SetActive(false);
        models[newIndex].SetActive(true);

        if (Object.HasInputAuthority)
            LobbyManager.Instance.CheckOwnerCharacterAvailable(Runner);
    }

    void ReadyChanged()
    {
        playerNameText.color = IsReady ? Color.green : Color.white;
        if (Object.HasInputAuthority)
        {
            nextCharacterButton.interactable = !IsReady;
            previousCharacterButton.interactable = !IsReady;
            if (IsReady)
                CharacterManager.Instance.CurrentCharacter = (CharacterColor)CurrentCharacterIndex;
        }
        else
        {
            LobbyManager.Instance.CheckOwnerCharacterAvailable(Runner);
        }

        if (Object.HasStateAuthority)
        {
            LobbyManager.Instance.StartGame(Runner, this);
        }
    }

    void NextCharacter()
    {
        int index = CurrentCharacterIndex + 1;
        if (index == models.Count)
        {
            index = 0;
        }
        RPC_SetCharacter(index);
    }

    void PreviousCharacter()
    {
        int index = CurrentCharacterIndex - 1;
        if (index == -1) {
            index = models.Count - 1;
        }
        RPC_SetCharacter(index);
    }

    void OnLeave()
    {
        LobbyManager.Instance.Shutdown(Runner);
    }

    void OnReady()
    {
        RPC_SetReady(!IsReady);
    }
}
