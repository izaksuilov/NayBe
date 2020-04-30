using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    public Image[] bgColorObjects;
    public Image[] activeColorObjects;
    public Image[] additionalObjects1;
    public Image[] additionalObjects2;
    public Text[] textColorObjects;
    public Text[] textOnBgColorObjects;
    private Color32[] bgColor = { new Color32(255, 255, 255, 255) };
    private Color32[] activeColor = { new Color32(43, 17, 90, 255) };
    private Color32[] additionalColor1 = { new Color32(118, 99, 154, 255) };
    private Color32[] additionalColor2 = { new Color32(246, 237, 255, 255) };
    private Color32[] textColor = { new Color32(246, 237, 255, 255) };
    private Color32[] textOnBgColor = { new Color32(31, 17, 52, 255) };
    private void Start()
    {
        SetColor();
    }
    public void SetColor()
    {
        int colorScheme = Settings.colorScheme;
        for (int i = 0; i < bgColorObjects.Length; i++)
            bgColorObjects[i].color = bgColor[colorScheme];

        for (int i = 0; i < activeColorObjects.Length; i++)
            activeColorObjects[i].color = activeColor[colorScheme];

        for (int i = 0; i < additionalObjects1.Length; i++)
            additionalObjects1[i].color = additionalColor1[colorScheme];

        for (int i = 0; i < additionalObjects2.Length; i++)
            additionalObjects2[i].color = additionalColor2[colorScheme];

        for (int i = 0; i < textColorObjects.Length; i++)
            textColorObjects[i].color = textColor[colorScheme];

        for (int i = 0; i < textOnBgColorObjects.Length; i++)
            textOnBgColorObjects[i].color = textOnBgColor[colorScheme];

    }
}
