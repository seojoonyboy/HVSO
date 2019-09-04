using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestRaycast : MonoBehaviour , IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject targetObject;    
    Vector3 objectPosition;


    public void OnBeginDrag(PointerEventData eventData) {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(
        new Vector2(mousePos.x, mousePos.y),
                Vector2.zero,
                Mathf.Infinity
            );



        if (results.Exists(x => x.gameObject.name == "TargetScope")) {
            if (results.Exists(x => x.gameObject.name == "TargetMask")) {

                if (hits.Length > 0) {
                    targetObject = hits[0].transform.gameObject;
                    objectPosition = targetObject.transform.position;
                }
                        
            }
        }   

    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (targetObject != null)
            targetObject.transform.position = mousePos;

    }

    public void OnEndDrag(PointerEventData eventData) {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Exists(x => x.gameObject.name == "TargetScope")) {
            if (results.Exists(x => x.gameObject.name == "TargetMask")) {

                if (targetObject != null) {
                    Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    targetObject.transform.position = position;
                }
            }
            else
                targetObject.transform.position = objectPosition;
        }
        else
            targetObject.transform.position = objectPosition;
        
        targetObject = null;
    }




    //public void OnPointerClick(PointerEventData eventData) {

    //    List<RaycastResult> result = new List<RaycastResult>();
    //    EventSystem.current.RaycastAll(eventData, result);

    //    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);       
    //    RaycastHit2D[] hits = Physics2D.RaycastAll(
    //        new Vector2(mousePos.x, mousePos.y),
    //        Vector2.zero,
    //        Mathf.Infinity
    //    );



    //    if (result.Exists(x => x.gameObject.name == "TargetScope")) {
    //        if (result.Exists(x => x.gameObject.name == "TargetMask")) {

    //            if (hits.Length > 0)
    //                Debug.Log(hits[0].transform.gameObject.name);
    //        }
    //    }


    //}



}
