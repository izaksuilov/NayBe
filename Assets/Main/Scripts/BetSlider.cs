using UnityEngine;
using UnityEngine.UI;

public class BetSlider : MonoBehaviour
{
    [SerializeField] private Slider sliderFrom;
    void Start()
    {
        SyncBets();
        FindObjectOfType<HubManager>().ApplyFilter();
    }
    public void SyncBets()
    {
        if (GetComponent<Slider>().value <= sliderFrom.value)
            GetComponent<Slider>().value = sliderFrom.value;
    }
    public void CheckValue()
    {
        if (GetComponent<Slider>().value < sliderFrom.value)
            sliderFrom.value = GetComponent<Slider>().value;
    }
}
