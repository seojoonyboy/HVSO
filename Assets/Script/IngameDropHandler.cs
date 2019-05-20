using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngameDropHandler : MonoBehaviour, IDropHandler
{
    public GameObject placedMonster;
    public GameObject ingameParent;

    private void Start()
    {
        ingameParent = PlayMangement.instance.player.transform.GetChild(gameObject.transform.parent.GetSiblingIndex()).GetChild(gameObject.transform.GetSiblingIndex()).transform.gameObject;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (placedMonster == null && PlayMangement.instance.player.getPlayerTurn == true) {
            int cardIndex = 0;
            if (eventData.pointerDrag.gameObject.transform.parent.name == "CardSlot_1")
                cardIndex = eventData.pointerDrag.gameObject.transform.GetSiblingIndex();
            else {
                cardIndex = GameObject.Find("CardSlot_2").transform.childCount + eventData.pointerDrag.gameObject.transform.GetSiblingIndex();
            }
            CardHandler cardHandler = eventData.pointerDrag.gameObject.GetComponent<CardHandler>();

            placedMonster = Instantiate(cardHandler.unit, ingameParent.transform);
            placedMonster.transform.position = ingameParent.transform.position;
            placedMonster.GetComponent<PlaceMonster>().isPlayer = true;

            placedMonster.GetComponent<PlaceMonster>().unit.name = cardHandler.cardData.name;
            placedMonster.GetComponent<PlaceMonster>().unit.HP = (int)cardHandler.cardData.hp;
            placedMonster.GetComponent<PlaceMonster>().unit.power = (int)cardHandler.cardData.attack;
            placedMonster.GetComponent<PlaceMonster>().unit.type = cardHandler.cardData.type;

            placedMonster.GetComponent<SpriteRenderer>().sprite = cardHandler.unitSprite;
            placedMonster.name = placedMonster.GetComponent<PlaceMonster>().unit.name;

            GetComponent<Image>().enabled = false;
            PlayMangement.instance.player.isPicking.Value = false;
            
            GameObject.Find("Player").transform.GetChild(0).GetComponent<PlayerController>().cdpm.DestroyCard(cardIndex);
            Debug.Log(cardIndex + " 번째 카드 제거");
        }
    }

    

}
