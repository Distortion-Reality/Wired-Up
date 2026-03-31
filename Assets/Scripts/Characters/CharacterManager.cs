using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    public GameObject redPrefab, bluePrefab, greenPrefab, purplePrefab, yellowPrefab;

    public Material redWireMaterial, blueWireMaterial, greenWireMaterial, purpleWireMaterial, yellowWireMaterial;
    
    CharacterColor currentCharacter = CharacterColor.Red;
    public CharacterColor CurrentCharacter
    {
        get => currentCharacter;
        set
        {
            currentCharacter = value;
            PlayerPrefs.SetInt(PlayerPrefKey.CharacterIndex, (int)currentCharacter);
        }
    }

    public GameObject GetPrefab(CharacterColor id)
    {
        switch (id)
        {
            case CharacterColor.Red:
                return redPrefab;
            case CharacterColor.Blue:
                return bluePrefab;
            case CharacterColor.Green:
                return greenPrefab;
            case CharacterColor.Purple:
                return purplePrefab;
            case CharacterColor.Yellow:
                return yellowPrefab;
            default:
                return redPrefab;
        }
    }

    public Material GetWireMaterial(CharacterColor id)
    {
        switch (id)
        {
            case CharacterColor.Red:
                return redWireMaterial;
            case CharacterColor.Blue:
                return blueWireMaterial;
            case CharacterColor.Green:
                return greenWireMaterial;
            case CharacterColor.Purple:
                return purpleWireMaterial;
            case CharacterColor.Yellow:
                return yellowWireMaterial;
            default:
                return redWireMaterial;
        }
    }

    void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefKey.CharacterIndex))
        {
            PlayerPrefs.SetInt(PlayerPrefKey.CharacterIndex, (int)currentCharacter);
        }
        else
        {
            currentCharacter = (CharacterColor)PlayerPrefs.GetInt(PlayerPrefKey.CharacterIndex);
        }
    }
}
