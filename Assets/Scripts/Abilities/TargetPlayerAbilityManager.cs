public class TargetPlayerAbilityManager : TargetAbilityManager
{
    protected override void CheckUserAbilityQueue(UserAbility userAbility)
    {
        StartCoroutine(DoAbilities());
    }
}
