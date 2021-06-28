using System.Collections.Generic;

public static class AbilityRegistry
{
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
}
