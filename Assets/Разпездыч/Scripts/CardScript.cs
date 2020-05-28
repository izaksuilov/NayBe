using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
            FieldType parentType = transform.parent.GetComponent<DropPlaceScript>().Type;
            if (RazManager.isBeginningPhase)
                transform.GetChild(0).GetChild(1).gameObject
                      .SetActive(parentType == FieldType.ENEMY_HAND || parentType == FieldType.MY_HAND);
                //.SetActive(true);
            else
                transform.GetChild(0).GetChild(1).gameObject.SetActive(parentType == FieldType.FIELD || parentType == FieldType.MY_HAND);
            transform.GetChild(0).GetChild(0).GetComponent<RawImage>().color = parentType == FieldType.UNASS ? new Color(0.4f, 0.4f, 0.4f) : Color.white;
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
        FieldType parentType = DefaultParent.GetComponent<DropPlaceScript>().Type;
        if (RazManager.isBeginningPhase)//если раздаём карты
            isDraggable = MapController.players[0].isPlayerTurn &&//если ход игрока
                          ((parentType == FieldType.MY_HAND && transform.GetSiblingIndex() == DefaultParent.childCount - 1) || //и если он берёт последнюю карту с руки
                           (parentType == FieldType.FIELD));//или если он берёт карту с поля
        else
            isDraggable = MapController.players[0].isPlayerTurn &&
                          (parentType == FieldType.MY_HAND ||
                          (parentType == FieldType.FIELD && transform.GetSiblingIndex() == 0));
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
        FieldType parentType = DefaultParent.GetComponent<DropPlaceScript>().Type;
        #region Проверка, что родитель определён верно
        if (RazManager.isBeginningPhase)
        {
            if (parentType != FieldType.MY_HAND && parentType != FieldType.ENEMY_HAND)//если кинул не пойми куда, то себе в руку 
                DefaultParent = MapController.FindChildrenWithTag(DefaultParent.parent.parent.gameObject, "HandPosition")[0].transform;
        }
        else 
        {
            if ((parentType != FieldType.MY_HAND && parentType != FieldType.FIELD)//если кинул не пойми куда
              || parentType == FieldType.FIELD && prevDefaultParent.GetComponent<DropPlaceScript>().Type == FieldType.FIELD)//или взял с поля, то себе в руку
                DefaultParent = MapController.FindChildrenWithTag(FindObjectOfType<Canvas>().gameObject, "HandPosition")[0].transform;
        }
        #endregion
        int activePlayers = 0;
        for (int i = MapController.players.Count - 1; i >= 0; i--)
            if (MapController.FindChildrenWithTag(MapController.players[i].transform.parent.parent.parent.gameObject, "HandPosition")[0].transform.childCount != 0) activePlayers++;
        #region Передача хода
        parentType = DefaultParent.GetComponent<DropPlaceScript>().Type;
        int currentCard = thisCard.Value,
                lastCard = DefaultParent.childCount != 0 ? DefaultParent.transform.GetChild(DefaultParent.childCount - 1).GetComponent<CardScript>().thisCard.Value : -1;
        if (RazManager.isBeginningPhase)
        {
            if (parentType == FieldType.MY_HAND && prevDefaultParent.GetComponent<DropPlaceScript>().Type != FieldType.MY_HAND && // если положил в свою руку не из своей руки
               (lastCard != -1 && // и если в руке были карты
               ((lastCard == 14 && currentCard != MapController.minCard) || (lastCard != 14 && (currentCard - lastCard != 1)))))// и если карта не идёт по иерархии
                    MapController.SwitchPlayerTurn();// то передать ход
        }
        else
        {
            if (parentType == FieldType.FIELD)
            {
                if (GameObject.Find("Inner Field").transform.childCount + 1 != activePlayers)
                    MapController.SwitchPlayerTurn();
            }
            else if ((parentType == FieldType.MY_HAND && prevDefaultParent.GetComponent<DropPlaceScript>().Type != FieldType.MY_HAND) ||
                      parentType != FieldType.MY_HAND)
                MapController.SwitchPlayerTurn();
        }
        #endregion

        transform.SetParent(DefaultParent);
        transform.localScale = new Vector3(1, 1, 1);
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        #region Синхронизация карт и игроков

        if (!RazManager.isBeginningPhase && prevDefaultParent.GetComponent<DropPlaceScript>().Type == FieldType.MY_HAND) // unass и окончание игры
        {
            GameObject unassPosition = MapController.FindChildrenWithTag(prevDefaultParent.transform.parent.gameObject, "UnAssPosition")[0];
            int player = MapController.FindChildrenWithTag(unassPosition.transform.parent.gameObject, "Player")[0].GetComponent<PhotonView>().OwnerActorNr;
            if (prevDefaultParent.childCount == 0)
            {
                MapController.MoveCard(thisCard.Value, thisCard.Suit);
                MapController.PlayerWin();
            }

            else if (prevDefaultParent.transform.childCount <= unassPosition.transform.childCount)
            {
                int childCount = unassPosition.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform unass = unassPosition.transform.GetChild(0);
                    unass.SetParent(MapController.FindChildrenWithTag(unass.parent.parent.gameObject, "HandPosition")[0].transform);
                    unass.localScale = new Vector3(1, 1, 1);
                    unass.localRotation = Quaternion.Euler(0, 0, 0);
                    unass.transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 0);
                    MapController.MoveCard(unass.GetComponent<CardScript>().thisCard.Value, unass.GetComponent<CardScript>().thisCard.Suit, player);
                }
            }
        }
        if (parentType == FieldType.FIELD)
        {
            MapController.MoveCard(thisCard.Value, thisCard.Suit);

            if (!RazManager.isBeginningPhase && transform.parent.childCount >= activePlayers)// когда карт столько же, сколько игроков, то удаляем карты
                StartCoroutine(MapController.CallClearField());
        }
        if ((RazManager.isBeginningPhase && parentType == FieldType.ENEMY_HAND) ||
            (parentType == FieldType.MY_HAND && prevDefaultParent.GetComponent<DropPlaceScript>().Type != FieldType.MY_HAND))
            MapController.MoveCard(thisCard.Value, thisCard.Suit,
                MapController.FindChildrenWithTag(transform.parent.parent.gameObject, "Player")[0].GetComponent<PhotonView>().OwnerActorNr);
        #endregion

        if (RazManager.isBeginningPhase && prevDefaultParent.GetComponent<DropPlaceScript>().Type == FieldType.FIELD &&
            prevDefaultParent.childCount == 0)// если осталась одна карта, то начинается вторая фаза игры
            MapController.StartSecondPhase(thisCard.Suit);
    }
}
