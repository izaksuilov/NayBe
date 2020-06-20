using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    public static Color32[] bgColor =          { new Color32(255, 255, 255, 255), new Color32(115, 155, 55 , 255) },
                            activeColor =      { new Color32(43 , 17 , 90 , 255), new Color32(243, 117, 90 , 255) },
                            additionalColor1 = { new Color32(118, 99 , 154, 255), new Color32(218, 249, 194, 255) },
                            additionalColor2 = { new Color32(246, 237, 255, 255), new Color32(146, 137, 255, 255) },
                            textColor =        { new Color32(246, 237, 255, 255), new Color32(146, 237, 155, 255) },
                            textOnBgColor =    { new Color32(31 , 17 , 52 , 255), new Color32(231, 217, 252, 255) };
    private void Update()
    {
        ApplyColor(Settings.colorScheme);
    }
    public void ApplyColor(int colorScheme)
    {
        foreach (GameObject obj in MapController.FindChildrenWithTag(FindObjectOfType<Canvas>().gameObject))
        {
            Color color = new Color();
            if (obj.GetComponent<Image>() != null)
                color = obj.GetComponent<Image>().color;
            else if (obj.GetComponent<Text>() != null)
                color = obj.GetComponent<Text>().color;
            else continue;

            if (color == bgColor[Settings.colorScheme]) color = bgColor[colorScheme];
            else if (color == activeColor[Settings.colorScheme]) color = activeColor[colorScheme];
            else if (color == additionalColor1[Settings.colorScheme]) color = additionalColor1[colorScheme];
            else if (color == additionalColor2[Settings.colorScheme]) color = additionalColor2[colorScheme];
            else if (color == textColor[Settings.colorScheme]) color = textColor[colorScheme];
            else if (color == textOnBgColor[Settings.colorScheme]) color = textOnBgColor[colorScheme];

            try { obj.GetComponent<Image>().color = color; }
            catch { obj.GetComponent<Text>().color = color; }
            Debug.Log(obj.name);
        }
        //Settings.SaveColor(colorScheme);
    }
}
