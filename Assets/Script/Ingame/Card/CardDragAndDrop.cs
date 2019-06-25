using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public Transform DragObserver;
    public CardCircleManager ListCircle;    
    private int myCardIndex;
    public int CARDINDEX {
        set { myCardIndex = value; }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (!ListCircle.dragable) return;
        transform.localScale = new Vector3(1.15f, 1.15f, 1);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);
        Vector3 observeMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        DragObserver.LookAt(observeMousePos, new Vector3(0, 0, 1));
        ListCircle.transform.SetParent(DragObserver);
    }

    public void OnDrag(PointerEventData eventData) {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);
        Vector3 observeMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        DragObserver.LookAt(observeMousePos, new Vector3(0, 0, 1));
        if (mousePos.y > -6.5f)
            transform.position = mousePos;
        else
            transform.localPosition = new Vector3(0, 4500, 0);
        
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (transform.localPosition.y > 5000) {
            StartCoroutine(ListCircle.UseCard(myCardIndex));
            ListCircle.transform.SetParent(DragObserver.parent);
        }
        else {
            transform.localScale = new Vector3(1, 1, 1);
            ListCircle.transform.SetParent(DragObserver.parent);
            transform.localPosition = new Vector3(0, 4500, 0);
            StartCoroutine(ListCircle.SortCircleAngle());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DragObserver = transform.parent.parent.Find("DragObserver");
        ListCircle = transform.parent.parent.Find("CardCircle").GetComponent< CardCircleManager>();
        gameObject.SetActive(false);
    }
}
