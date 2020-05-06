using UnityEngine;

public class ContentScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, 100 + transform.childCount * 500);
    }
}
