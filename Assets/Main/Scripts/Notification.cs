using System;
using UnityEngine;
using UnityEngine.UI;
public class Notification : MonoBehaviour {
    public enum Position
    {
        top,
        bottom
    };
    public static void ShowMessage ( string msg, float time = 1f, Notification.Position position = Notification.Position.top )
    {
        if (GameObject.Find("Message(Clone)") != null) return;
        GameObject messagePrefab = Resources.Load("Message") as GameObject;
        //Get container object of message
        GameObject containerObject = messagePrefab.gameObject.transform.GetChild ( 0 ).gameObject;
        //Get text object
        GameObject textObject = containerObject.gameObject.transform.GetChild ( 0 ).GetChild ( 0 ).gameObject;
        //Get text property
        Text msg_text = textObject.GetComponent<Text> ( );
        //Set message to text ui
        msg_text.text = msg;
        //Set position of container object of message
        SetPosition ( containerObject.GetComponent<RectTransform> ( ), position );
        //Spawn message object with all changes
        GameObject clone = Instantiate (messagePrefab);
        // Destroy clone of message object according to the time
        Destroy(clone.gameObject, time);
    }

    private static void SetPosition ( RectTransform rectTransform, Position position )
    {
        if (position == Position.top)
        {
            rectTransform.anchorMin = new Vector2 ( 0.5f, 1f );
            rectTransform.anchorMax = new Vector2 ( 0.5f, 1f );
            rectTransform.anchoredPosition = new Vector3 ( 0.5f, -150, 0 );
        }
        else
        {
            rectTransform.anchorMin = new Vector2 ( 0.5f, 0 );
            rectTransform.anchorMax = new Vector2 ( 0.5f, 0 );
            rectTransform.anchoredPosition = new Vector3 ( 0.5f, 150, 0 );
        }
    }
}
