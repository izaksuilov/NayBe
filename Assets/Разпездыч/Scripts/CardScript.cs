using UnityEngine;
using UnityEngine.EventSystems;

public struct Card
{
    public GameObject Obj;
    public int Value;
    public string Suit;
    public Card(GameObject obj, int value, string suit)
    {
        Obj = obj;
        Value = value;
        Suit = suit;
    }
}

public class CardScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Camera mainCamera;
    public Transform DefaultParent;
    bool isDraggable;
    public Card thisCard { get; private set; }
    
    void Update()
    {
        try
        {
            if (RazManager.isBeginningPhase)
            {
                transform.GetChild(0).GetChild(1).gameObject
                    .SetActive(transform.parent.GetComponent<DropPlaceScript>().Type == FieldType.MY_HAND 
                    || transform.parent.GetComponent<DropPlaceScript>().Type == FieldType.ENEMY_HAND);
            }
        }
        catch { }
    }
    void Awake()
    {
        mainCamera = Camera.allCameras[0];
    }
    public void CreateCard(Card card)
    {
        thisCard = card;
        MapController.allCards.Add(thisCard);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        DefaultParent = transform.parent;
        if (RazManager.isBeginningPhase)
            isDraggable = (DefaultParent.GetComponent<DropPlaceScript>().Type == FieldType.MY_HAND)
                          || (DefaultParent.GetComponent<DropPlaceScript>().Type == FieldType.FIELD);
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
        transform.localScale = new Vector3(1, 1, 1);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
