using UnityEngine.UI;
using UnityEngine;

public class NumberScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Text myText;
    // Start is called before the first frame update
    private void Start()
    {
        ChangeNum();
    }
    public void ChangeNum()
    {
        myText.text = slider.value.ToString();
    }
}
