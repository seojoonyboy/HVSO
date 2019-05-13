using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

public class CardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector3 startPos;
    public GameObject unit;
    private bool blockButton = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        blockButton = true;
        startPos = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.position = startPos;
        blockButton = false;
    }

    public void OpenCardInfoList() {
        if (!blockButton) {
            Transform css = GameObject.Find("Canvas").transform.GetChild(3);
            css.GetComponentInChildren<HorizontalScrollSnap>().GoToScreen(transform.GetSiblingIndex());
            css.gameObject.SetActive(true);
        }
    }
}
