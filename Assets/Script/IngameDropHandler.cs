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
            Destroy(eventData.pointerDrag.gameObject);
        }
    }

    

}
