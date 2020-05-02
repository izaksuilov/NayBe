using UnityEngine;
using UnityEngine.UI;

public class ToggleBtnTextColor : MonoBehaviour
{
    public void ChangeColor()
    {
        Color32 darkColor = ColorManager.additionalColor1[Settings.colorScheme];
        Color32 lightColor = ColorManager.textColor[Settings.colorScheme];
        if (GetComponent<Text>().color.Equals(darkColor))
            GetComponent<Text>().color = lightColor;
        else GetComponent<Text>().color = darkColor;
    }
}
