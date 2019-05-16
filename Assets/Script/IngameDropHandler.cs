using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
            placedMonster = Instantiate(eventData.pointerDrag.gameObject.GetComponent<CardHandler>().unit);
            placedMonster.transform.SetParent(ingameParent.transform);
            placedMonster.transform.position = ingameParent.transform.position;
            placedMonster.GetComponent<PlaceMonster>().isPlayer = true;
            //PlayMangement.instance.player.placement[gameObject.transform.parent.GetSiblingIndex(), gameObject.transform.GetSiblingIndex()] = placedMonster.GetComponent<PlaceMonster>().unit.id;
            Debug.Log(PlayMangement.instance.player.placement);
            Destroy(eventData.pointerDrag.gameObject);
        }
    }

    

}
