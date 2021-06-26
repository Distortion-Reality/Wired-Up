using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public GameObject redPrefab, bluePrefab, greenPrefab, purplePrefab, yellowPrefab;
    
    CharacterColor currentCharacter;
    public CharacterColor CurrentCharacter { get => currentCharacter; set => currentCharacter = value; }

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

    void Awake() 
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}
