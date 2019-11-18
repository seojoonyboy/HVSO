using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EditBarDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField] public Transform topDragLimit;
    [SerializeField] public Transform bottomDragLimit;
    [SerializeField] public Transform bookBottomLimit;
    [SerializeField] public Transform defaultDragLimit;
    [SerializeField] public Transform handTopPos;
    [SerializeField] public Transform handDeckArea;
    [SerializeField] public Transform cardBookArea;
    [SerializeField] public Transform deckEditCanvas;

    float canvasScale;

    public void InitCanvas() {
        canvasScale = deckEditCanvas.localScale.x;
        transform.position = new Vector3(transform.position.x, defaultDragLimit.position.y - (55 * canvasScale), 0);
        handDeckArea.GetChild(0).localPosition = Vector3.zero;
        cardBookArea.GetChild(0).localPosition = Vector3.zero;
        SetAreas();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardBookArea.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(handDeckArea.GetComponent<RectTransform>());
    }

    public void OnBeginDrag(PointerEventData eventData) {
        SetAreas();
    }

    public void OnDrag(PointerEventData eventData) {
        float mousePosY = Input.mousePosition.y;
        transform.position = new Vector3(transform.position.x, mousePosY);
        if (transform.Find("CardBookBottom").position.y < bottomDragLimit.position.y)
            transform.position = new Vector3(transform.position.x, bottomDragLimit.position.y + (55 * canvasScale), 0);
        if (transform.Find("CardBookTop").position.y > topDragLimit.position.y)
            transform.position = new Vector3(transform.position.x, topDragLimit.position.y - (55 * canvasScale), 0);

        SetAreas();
    }


    public void OnEndDrag(PointerEventData eventData) {
        if (transform.Find("CardBookBottom").position.y < bottomDragLimit.position.y)
            transform.position = new Vector3(transform.position.x, bottomDragLimit.position.y + (55 * canvasScale), 0);
        if (transform.Find("CardBookTop").position.y > topDragLimit.position.y)
            transform.position = new Vector3(transform.position.x, topDragLimit.position.y - (55 * canvasScale), 0);

        SetAreas();
    }

    public void SetAreas() {
        RectTransform bookRect = cardBookArea.GetComponent<RectTransform>();
        float yPos = ((transform.Find("CardBookBottom").position.y + bookBottomLimit.position.y) / 2);
        float newHeight = (bookBottomLimit.position.y - transform.Find("CardBookBottom").position.y) / canvasScale;
        cardBookArea.position = new Vector3(cardBookArea.position.x, yPos, 0);
        if (newHeight < 0)
            newHeight = newHeight * -1;
        bookRect.sizeDelta = new Vector2(bookRect.sizeDelta.x, newHeight);


        RectTransform handRect = handDeckArea.GetComponent<RectTransform>();
        yPos = ((transform.Find("CardBookTop").position.y + handTopPos.position.y) / 2);
        newHeight = (handTopPos.position.y - transform.Find("CardBookTop").position.y) / canvasScale;
        handDeckArea.position = new Vector3(handDeckArea.position.x, yPos, 0);
        if (newHeight < 0)
            newHeight = newHeight * -1;
        handRect.sizeDelta = new Vector2(handRect.sizeDelta.x, newHeight);
    }
}
