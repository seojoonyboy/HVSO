using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardDragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler 
{
    public Transform DragObserver;
    public Transform ListCircle;
    Quaternion startRotation;
    public void OnBeginDrag(PointerEventData eventData) {
        transform.localScale = new Vector3(1.15f, 1.15f, 1);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0);
        Vector3 observeMousePos = new Vector3(mousePos.x, mousePos.y, 0);
        DragObserver.LookAt(observeMousePos, new Vector3(0, 0, 1));
        startRotation = ListCircle.localRotation;
        ListCircle.SetParent(DragObserver);
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
        Debug.Log(mousePos);
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.localScale = new Vector3(1, 1, 1);
        ListCircle.SetParent(DragObserver.parent);
        transform.localPosition = new Vector3(0, 4500, 0);
        //ListCircle.LookAt(ListCircle.forward, new Vector3(0, 0, -1));
        if (ListCircle.rotation.eulerAngles.z > 300 && ListCircle.rotation.eulerAngles.z < 344) {
            //ListCircle.rotation = new Quaternion(0, 0, -0.0544f, 1);
            iTween.RotateTo(ListCircle.gameObject, new Vector3(0, 0, -16), 0.2f) ;
        }
        if (ListCircle.rotation.eulerAngles.z < 60 && ListCircle.rotation.eulerAngles.z > 8)
            iTween.RotateTo(ListCircle.gameObject, new Vector3(0, 0, 12), 0.2f);
        //if (ListCircle.GetComponent<RectTransform>().rotation.z > 10) {
        //    ListCircle.rotation = startRotation;
        //    ListCircle.Rotate(0, 0, 10);
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        DragObserver = transform.parent.parent.parent.Find("DragObserver");
        ListCircle = transform.parent.parent;
        if (transform.parent.name == "testa" || transform.parent.name == "testb")
            Debug.Log(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
