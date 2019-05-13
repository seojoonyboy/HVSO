using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IngameDropHandler : MonoBehaviour, IDropHandler
{
    public GameObject placed;
    public GameObject ingameParent;

    private void Start()
    {
        ingameParent = PlayMangement.instance.player.transform.GetChild(gameObject.transform.parent.GetSiblingIndex()).GetChild(gameObject.transform.GetSiblingIndex()).transform.gameObject;
    }

    public void OnDrop(PointerEventData eventData)
    {        
        placed = Instantiate(eventData.pointerDrag.gameObject.GetComponent<CardHandler>().unit);
        placed.transform.SetParent(ingameParent.transform);
        placed.transform.position = ingameParent.transform.position;
    }

    

}
