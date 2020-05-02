using UnityEngine.UI;
using UnityEngine;

public class NumberScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private void Start()
    {
        ChangeNum();
    }
    public void ChangeNum()
    {
        GetComponent<Text>().text = slider.value.ToString();
    }
}
