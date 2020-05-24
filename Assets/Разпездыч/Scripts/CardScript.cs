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
    public Transform DefaultParent, prevDefaultParent;
    bool isDraggable;
    public Card thisCard { get; private set; }
    
    void Update()
    {
        try
        {
            if (RazManager.isBeginningPhase)
            {
                transform.GetChild(0).GetChild(1).gameObject
                      //.SetActive(transform.parent.GetComponent<DropPlaceScript>().Type == FieldType.MY_HAND
                      //|| transform.parent.GetComponent<DropPlaceScript>().Type == FieldType.ENEMY_HAND);
                      .SetActive(true);
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
        prevDefaultParent = DefaultParent = transform.parent;
        if (RazManager.isBeginningPhase)//если раздаём карты
            isDraggable = MapController.players[0].isPlayerTurn &&//если ход игрока
                          ((DefaultParent.GetComponent<DropPlaceScript>().Type == FieldType.MY_HAND && transform.GetSiblingIndex() == DefaultParent.childCount - 1) || //и если он берёт последнюю карту с руки
                          (DefaultParent.GetComponent<DropPlaceScript>().Type == FieldType.FIELD));//или если он берёт карту с поля

        if (!isDraggable) return;
        transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
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
        if (RazManager.isBeginningPhase)
        {
            FieldType parentType = DefaultParent.GetComponent<DropPlaceScript>().Type;
            if (parentType != FieldType.MY_HAND && parentType != FieldType.ENEMY_HAND)
            {
                DefaultParent = MapController.FindChildrenWithTag(DefaultParent.parent.parent.gameObject, "HandPosition")[0].transform;
                parentType = FieldType.MY_HAND;
            }
            int currentCard = thisCard.Value, 
                lastCard = DefaultParent.childCount != 0 ? DefaultParent.transform.GetChild(DefaultParent.childCount - 1).GetComponent<CardScript>().thisCard.Value : -1;
            if (parentType == FieldType.MY_HAND && prevDefaultParent.GetComponent<DropPlaceScript>().Type != FieldType.MY_HAND && // если положил в свою руку не из своей руки
                (lastCard != -1 && // и если в руке были карты
                ((lastCard == 14 && currentCard != MapController.minCard) || (lastCard != 14 && (currentCard - lastCard != 1)))))// и если карта не идёт по иерархии
                    MapController.SwitchPlayerTurn();// то передать ход
        }
        transform.SetParent(DefaultParent);
        transform.localScale = new Vector3(1, 1, 1);
        //MapController.MoveCard();
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
