using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Peripherals Prefabs")]
    public GameObject blue;
    public GameObject red;
    public GameObject green;
    public GameObject yellow;
    
    [Header("Bosses Prefabs")]
    public GameObject kirin;

    public GameObject GetPrefab(EnemyId id)
    {
        return id switch
        {
            EnemyId.BluePeripheral => blue,
            EnemyId.GreenPeripheral => green,
            EnemyId.RedPeripheral => red,
            EnemyId.YellowPeripheral => yellow,
            EnemyId.Kirin => kirin,
            _ => null,
        };
    }
}
