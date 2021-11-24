using System.Collections.Generic;

public static class AbilityRegistry
{
    public struct CharacterAbilities
    {
        public List<Ability> attacks;
        public List<Ability> assists;
    }

    readonly static Dictionary<AbilityId, Ability> registry = new Dictionary<AbilityId, Ability>
    {
        { AbilityId.BlueAssist2, new BlueAssist2() },
        { AbilityId.GreenAttack1, new GreenAttack1() },
        { AbilityId.GreenAttack3, new GreenAttack3() },
        { AbilityId.GreenAssist3, new GreenAssist3() },
        { AbilityId.PurpleAttack1, new PurpleAttack1() },
        { AbilityId.PurpleAttack2, new PurpleAttack2() },
        { AbilityId.PurpleAssist3, new PurpleAssist3() },
        { AbilityId.RedAttack1, new RedAttack1() },
        { AbilityId.RedAttack2, new RedAttack2() }
    };

    public static Ability Get(AbilityId id)
    {
        return registry[id];    
    }

    public static CharacterAbilities GetAbilities(CharacterColor characterColor)
    {
        List<Ability> attacks;
        List<Ability> assists;

        switch (characterColor)
        {
            case CharacterColor.Blue:
                attacks = new List<Ability> { Get(AbilityId.RedAttack1), Get(AbilityId.RedAttack2) };
                assists = new List<Ability> { Get(AbilityId.BlueAssist2) };
                break;
            case CharacterColor.Green:
                attacks = new List<Ability> { Get(AbilityId.GreenAttack1), Get(AbilityId.GreenAttack3) };
                assists = new List<Ability> { Get(AbilityId.GreenAssist3) };
                break;
            case CharacterColor.Purple:
                attacks = new List<Ability> { Get(AbilityId.PurpleAttack1), Get(AbilityId.PurpleAttack2) };
                assists = new List<Ability> { Get(AbilityId.PurpleAssist3) };
                break;
            case CharacterColor.Red:
                attacks = new List<Ability> { Get(AbilityId.RedAttack1), Get(AbilityId.RedAttack2) };
                assists = new List<Ability> { Get(AbilityId.BlueAssist2) };
                break;
            case CharacterColor.Yellow:
                attacks = new List<Ability> { Get(AbilityId.RedAttack1), Get(AbilityId.RedAttack2) };
                assists = new List<Ability> { Get(AbilityId.BlueAssist2) };
                break;
            default:
                attacks = new List<Ability>();
                assists = new List<Ability>();
                break;
        }

        return new CharacterAbilities { attacks = attacks, assists = assists };
    }
}
