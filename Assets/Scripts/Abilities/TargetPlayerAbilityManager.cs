public class TargetPlayerAbilityManager : TargetAbilityManager
{
    protected override void CheckUserAbilityQueue(Fighter user)
    {
        StartCoroutine(DoAbilities());
    }
}
