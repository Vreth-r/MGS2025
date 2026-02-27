using UnityEngine;
using UnityEngine.UI;

public class UltimateManager : MonoBehaviour
{
    public int ult = 0;
    public int maxUlt = 10;

    public Ultimate ultBar;

    public void Start()
    {
        ultBar.SetBaseUlt();
    }

    public void Update()
    {
        if (ult < maxUlt)
            AddUlt(1);
            Debug.Log("Gained ultimate");
    }

    void AddUlt(int gain)
    {
        ult += gain;
        ultBar.GainUlt(ult);
    }
}
