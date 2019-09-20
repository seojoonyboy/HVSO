using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditCardDragHandler : EditCardHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public void OnBeginDrag(PointerEventData eventData) {
        if (Input.touchCount > 1) return;
        if (disabled) return;
        if (!dragable) return;
        if (onAnimation) return;
        draggingObject = gameObject;
        dragable = false;
        dragging = true;
        startPos = transform.position;
        startLocalPos = transform.localPosition;
        if (deckEditController.setCardList.Count > 4) {
            Vector3 mousePos = Input.mousePosition;
            mouseFirstXPos = mousePos.x;
            handFirstXPos = transform.parent.position.x;
        }
    }

    public void OnDrag(PointerEventData eventData) {        
        if (Input.touchCount > 1) return;
        if (disabled) return;
        if (draggingObject != gameObject) return;
        standby = false;
        if (deckEditController.setCardList.Count > 4) {
            Vector3 mousePos = Input.mousePosition;
            float mouseMoved = mousePos.x - mouseFirstXPos;
            transform.parent.position = new Vector3(handFirstXPos + mouseMoved, transform.parent.position.y, 0);
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (disabled) return;
        if (!dragging) return;
        if (draggingObject != gameObject) return;
        if (onAnimation) return;
        if (deckEditController.setCardList.Count > 4) {
            if (transform.parent.GetChild(0).position.x > transform.parent.parent.Find("CenterPos").position.x)
                iTween.MoveTo(deckEditController.settingLayout.gameObject, iTween.Hash("x", 0, "y", -550, "islocal", true, "time", 0.2f));
            if (transform.parent.GetChild(deckEditController.setCardList.Count - 1).position.x < transform.parent.parent.Find("CenterPos").position.x)
                iTween.MoveTo(deckEditController.settingLayout.gameObject, iTween.Hash("x", -240 * (deckEditController.setCardList.Count - 4), "y", -550, "islocal", true, "time", 0.2f));
        }
        if (!onAnimation)
            dragable = true;
        dragging = false;
        draggingObject = null;
    }
}
