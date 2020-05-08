using UnityEngine;
using UnityEngine.EventSystems;

public class CardScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Camera mainCamera;
    public Transform DefaultParent;
    bool isDraggable;
    void Update()
    {
        try
        {
            transform.GetChild(0).GetChild(1).gameObject
                .SetActive(transform.parent.GetComponent<DropPlaceScript>().Type != FieldType.OTHER);
        }
        catch { }
        
    }
    void Awake()
    {
        mainCamera = Camera.allCameras[0];
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        DefaultParent = transform.parent;
        isDraggable = DefaultParent.GetComponent<DropPlaceScript>().Type == FieldType.MY_HAND;
        if (!isDraggable) return;

        transform.GetChild(0).rotation = Quaternion.Euler(0, 0, 0);
        transform.SetParent(DefaultParent.parent.parent);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        Vector3 newPos = mainCamera.ScreenToWorldPoint(eventData.position);
        newPos.z = 0;
        transform.position = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        transform.SetParent(DefaultParent);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
