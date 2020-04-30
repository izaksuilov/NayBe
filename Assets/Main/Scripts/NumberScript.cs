using UnityEngine.UI;
using UnityEngine;

public class NumberScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    // Start is called before the first frame update
    private void Start()
    {
        ChangeNum();
    }
    public void ChangeNum()
    {
        this.GetComponent<Text>().text = slider.value.ToString();
    }
}
