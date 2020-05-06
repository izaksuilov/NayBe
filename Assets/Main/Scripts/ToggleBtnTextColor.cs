using UnityEngine;
using UnityEngine.UI;

public class ToggleBtnTextColor : MonoBehaviour
{
    public void ChangeColor(bool isInversed)
    {
        
        Color32 darkColor = ColorManager.additionalColor1[Settings.colorScheme];
        Color32 lightColor = ColorManager.textColor[Settings.colorScheme];
        if (isInversed)
        {
            darkColor = ColorManager.textColor[Settings.colorScheme];
            lightColor = ColorManager.activeColor[Settings.colorScheme];
        }
        if (GetComponent<Toggle>().isOn == true)
        {
            try
            {
                GetComponent<Toggle>().gameObject.transform.GetChild(1).GetComponent<Image>().color = lightColor;
            }
            
        }
            
        else GetComponent<Toggle>().gameObject.transform.GetChild(1).GetComponent<Text>().color = darkColor;
    }
}
