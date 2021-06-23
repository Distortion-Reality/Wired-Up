using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public enum CharacterColor
    {
        Red, Blue, Green, Purple, Yellow
    }

    public GameObject redPrefab, bluePrefab, greenPrefab, purplePrefab, yellowPrefab;

    public CharacterColor[] GetCharacterColors()
    {
        return (CharacterColor[]) Enum.GetValues(typeof(CharacterColor));
    }

    public GameObject GetPrefab(CharacterColor type)
    {
        switch (type)
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
