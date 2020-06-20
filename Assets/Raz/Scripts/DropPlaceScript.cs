using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum FieldType
{
    MY_HAND,
    FIELD,
    ENEMY_HAND,
    UNASS
}
public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    public FieldType Type;
    private void Update()
    {
        if (Type == FieldType.FIELD)
        {
            int distance;
            switch (transform.childCount)
            {
                case 1:
                case 2: distance = -705; break;
                case 3: distance = -570; break;
                default: distance = -350; break;
            }
            transform.GetComponent<HorizontalLayoutGroup>().spacing = distance;
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        CardScript card = eventData.pointerDrag.GetComponent<CardScript>();
        if (transform.childCount == 0) goto end;
        Card currentCard = card.thisCard, 
        lastCard = transform.GetChild(transform.childCount - 1).GetComponent<CardScript>().thisCard;
        if (RazManager.isBeginningPhase)//если раздём карты, то нужно проверить, как мы их положили
        {
            if (Type == FieldType.ENEMY_HAND || Type == FieldType.MY_HAND)
            {
                if (lastCard.Value == 14)
                {
                    if (currentCard.Value != MapController.minCard) return;
                }
                else if (currentCard.Value - lastCard.Value != 1) return;
            }
            else return;
        }
        else // если играем
        {
            if (Type == FieldType.FIELD && 
                card.prevDefaultParent.GetComponent<DropPlaceScript>().Type != FieldType.FIELD)
            {
                if (transform.childCount >= MapController.players.Count) return;
                if (lastCard.Suit.Equals("pik") && !currentCard.Suit.Equals("pik"))// пики бьют только пики
                    return;
                if (currentCard.Value > lastCard.Value)
                {
                    if (!currentCard.Suit.Equals(lastCard.Suit) && !currentCard.Suit.Equals(RazManager.ace))
                        return;
                }
                else 
                {
                    if (lastCard.Suit.Equals(RazManager.ace) || 
                      (!lastCard.Suit.Equals(RazManager.ace) && !currentCard.Suit.Equals(RazManager.ace))) return;
                }
            }
            else if (Type != FieldType.MY_HAND) return;
        }
        end:
        if (card)
            card.DefaultParent = transform;
    }
}
