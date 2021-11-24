using System;
using UnityEngine;

public enum CharacterColor
{
    Red, Blue, Green, Purple, Yellow
}

static class CharacterColorHelper
{
    public static Color UnityColor(this CharacterColor id)
    {
        switch (id)
        {
            case CharacterColor.Red:
                return Color.red;
            case CharacterColor.Blue:
                return Color.blue;
            case CharacterColor.Green:
                return Color.green;
            case CharacterColor.Purple:
                return Color.magenta;
            case CharacterColor.Yellow:
                return Color.yellow;
            default:
                return Color.red;
        }
    }
    public static CharacterColor[] Values()
    {
        return (CharacterColor[]) Enum.GetValues(typeof(CharacterColor));
    }
}