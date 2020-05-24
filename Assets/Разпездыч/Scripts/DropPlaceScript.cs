using UnityEngine;
using UnityEngine.EventSystems;

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
    public void OnDrop(PointerEventData eventData)
    {
        //if (!RazManager.isBeginningPhase && (Type == FieldType.ENEMY_HAND || Type == FieldType.UNASS)) return;
        CardScript card = eventData.pointerDrag.GetComponent<CardScript>();
        if (RazManager.isBeginningPhase)//если раздём карты, то нужно проверить, как мы их положили
        {
            int currentCard = card.thisCard.Value, lastCard = transform.GetChild(transform.childCount - 1).GetComponent<CardScript>().thisCard.Value;
            if (Type == FieldType.ENEMY_HAND || Type == FieldType.MY_HAND)
            {
                if (lastCard == 14)
                {
                    if (currentCard != MapController.minCard) return;
                } 
                else if (currentCard - lastCard != 1) return;
            }
        }
        if (card)
            card.DefaultParent = transform;
        
    }
}
