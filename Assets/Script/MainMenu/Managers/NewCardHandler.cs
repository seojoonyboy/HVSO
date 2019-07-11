using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewCardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField] GameObject cardHand;
    [SerializeField] GameObject mouseXPos;

    public void OnBeginDrag(PointerEventData eventData) {
        transform.localScale = new Vector3(1.15f, 1.15f, 1);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);
        Vector3 observeMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        mouseXPos.transform.position = new Vector3(mousePos.x, 0, 0);
        cardHand.transform.SetParent(mouseXPos.transform);
    }

    public void OnDrag(PointerEventData eventData) {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);
        Vector3 observeMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        mouseXPos.transform.position = new Vector3(mousePos.x, 0, 0);
        if (mousePos.y > -6.5f)
            transform.position = mousePos;
        else
            transform.localPosition = new Vector3(0, 0, 0);
    }

    public void OnEndDrag(PointerEventData eventData) {
        cardHand.transform.SetParent(mouseXPos.transform.parent);
        transform.localScale = new Vector3(1, 1, 1);
        transform.localPosition = new Vector3(0, 0, 0);
    }
}
