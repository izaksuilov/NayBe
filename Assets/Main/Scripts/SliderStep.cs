using UnityEngine;
using UnityEngine.UI;

public class SliderStep : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private void Start()
    {
        ChangeNumber();
    }
    public void ChangeNumber()
    {
        float value = slider.value;
        switch(value)
        {
            case 1: value = 100; break;
            case 2: value = 250; break;
            case 3: value = 500; break;
            case 4: value = 1000; break;
            case 5: value = 1500; break;
            case 6: value = 2000; break;
            case 7: value = 3000; break;
            case 8: value = 5000; break;
            case 9: value = 10000; break;
            case 10: value = 15000; break;
            case 11: value = 25000; break;
            case 12: value = 50000; break;
            case 13: value = 75000; break;
            case 14: value = 100000; break;
            case 15: value = 250000; break;
            case 16: value = 500000; break;
            case 17: value = 1000000; break;
        }
        if (value > Settings.money)
        {
            slider.value = slider.value - 1;
            Notification.ShowMessage("У Вас недостаточно денег", 2f);
            return;
        }
        GetComponent<Text>().text = value.ToString();
    }
}
