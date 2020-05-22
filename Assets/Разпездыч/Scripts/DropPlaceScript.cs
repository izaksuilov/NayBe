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
        if (RazManager.isBeginningPhase)
        {
            if (Type == FieldType.ENEMY_HAND)
            {
                int c1 = card.thisCard.Value, c2 = transform.GetChild(transform.childCount - 1).GetComponent<CardScript>().thisCard.Value;
                //if (c1 == 14 && c2 != )
                if (card.thisCard.Value - c2 != 1) return;
            }
            else if (Type != FieldType.MY_HAND) return;
        }
        if (card)
            card.DefaultParent = transform;
    }
}
