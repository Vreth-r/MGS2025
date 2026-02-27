using UnityEngine;
using UnityEngine.UI;

public class Ultimate : MonoBehaviour
{
    public Slider slider;

    public void SetBaseUlt()
    {
        slider.value = 0;
    }

    public void GainUlt(int ult)
    {
        slider.value = ult;
    }
}
