using UnityEngine;
using UnityEngine.UI;

public class ToggleBtnTextColor : MonoBehaviour
{
    public void ChangeColor(bool isInversed)
    {

        Color32 darkColor = ColorManager.additionalColor1[Settings.ColorScheme];
        Color32 lightColor = ColorManager.textColor[Settings.ColorScheme];
        if (isInversed)
        {
            darkColor = ColorManager.textColor[Settings.ColorScheme];
            lightColor = ColorManager.activeColor[Settings.ColorScheme];
        }
        if (GetComponent<Toggle>().isOn == true)
        {
            try { GetComponent<Toggle>().gameObject.transform.GetChild(1).GetComponent<Text>().color = lightColor; }
            catch { GetComponent<Toggle>().gameObject.transform.GetChild(1).GetComponent<Image>().color = lightColor; }
        }
        else
        {
            try { GetComponent<Toggle>().gameObject.transform.GetChild(1).GetComponent<Text>().color = darkColor; }
            catch { GetComponent<Toggle>().gameObject.transform.GetChild(1).GetComponent<Image>().color = darkColor; }
        }
    }
}
