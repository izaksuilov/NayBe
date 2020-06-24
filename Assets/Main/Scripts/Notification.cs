using System;
using UnityEngine;
using UnityEngine.UI;
public class Notification : MonoBehaviour {
    public enum Color
    {
        good,
        bad
    }
    public enum Position
    {
        top,
        bottom
    };
    public static void Show ( string msg, float time = 1f, Notification.Position position = Notification.Position.top, Notification.Color color = Notification.Color.bad )
    {
        if (GameObject.Find("Notification(Clone)") != null) return;
        GameObject notificationPrefab = Resources.Load("Notification") as GameObject,
                   container = notificationPrefab.gameObject.transform.GetChild(0).gameObject;
        container.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text = msg;
        SetPosition(container.GetComponent<RectTransform>(), position);
        SetColor(container.transform.GetChild(0).GetComponent<Image>(), color);
        GameObject clone = Instantiate (notificationPrefab);
        Destroy(clone.gameObject, time);
    }

    private static void SetPosition ( RectTransform rectTransform, Position position )
    {
        switch(position)
        {
            case Position.top:
            {
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.anchoredPosition = new Vector3(0.5f, -150, 0);
                break;
            }
            case Position.bottom:
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 0);
                rectTransform.anchoredPosition = new Vector3(0.5f, 150, 0);
                break;
            }
        }
    }
    private static void SetColor(Image bg, Color color)
    {
        switch (color)
        {
            case Color.good:
            {
                bg.color = new Color32(29, 90, 17, 255);
                break;
            }
            case Color.bad:
            {
                bg.color = new Color32(90, 17, 19, 255);
                break;
            }
        }
    }
}
