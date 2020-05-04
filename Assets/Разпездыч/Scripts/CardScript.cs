using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Camera mainCamera;
    Vector3 offset;
    void Awake()
    {
        mainCamera = Camera.allCameras[0];
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.position - mainCamera.ScreenToWorldPoint(eventData.position);
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 newPos = mainCamera.ScreenToWorldPoint(eventData.position);
        newPos.z = 0;
        transform.position = newPos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }
    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer
            || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            foreach(Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Stationary)
                {
                    StartCoroutine(ShowCard());
                }
            }
        }
    }
    IEnumerator ShowCard()
    {
        int i = 1;
        while (transform.GetChild(0).rotation.eulerAngles.z % 365 > 15)
        {
            float z = transform.GetChild(0).rotation.eulerAngles.z;
            Debug.Log(z);
            transform.GetChild(0).rotation
                = Quaternion.Euler(0, 0, z > 180 ? z + i : z - i);
            i++;
            yield return new WaitForSeconds(0.01f);
        }
        transform.GetChild(0).rotation = Quaternion.Euler(0, 0, 0);

    }
}
