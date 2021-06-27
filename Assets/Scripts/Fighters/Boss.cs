using UnityEngine;

public class Boss : Enemy
{
    string bossName = "Kirin";

    public override void EntityStart()
    {
        base.EntityStart();

        healthBar.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = bossName;
    }

    protected override void UpdateHealthBarTransform()
    {
        // Health bar is fixed on canvas
    }
}
