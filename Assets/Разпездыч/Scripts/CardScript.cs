using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Camera mainCamera;
    public Transform DefaultParent;
    void Awake()
    {
        mainCamera = Camera.allCameras[0];
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.GetChild(0).rotation = Quaternion.Euler(0, 0, 0);
        DefaultParent = transform.parent;
        transform.SetParent(DefaultParent.parent.parent);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 newPos = mainCamera.ScreenToWorldPoint(eventData.position);
        newPos.z = 0;
        transform.position = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(DefaultParent);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
