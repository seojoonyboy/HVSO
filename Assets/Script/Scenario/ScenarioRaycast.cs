using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScenarioRaycast : MonoBehaviour , IPointerDownHandler
{
    //public static ScenarioRaycast Instance { get; protected set; }
    //public GameObject targetObject;
    //public GameObject targetLocation;

    //public GameObject selectObject;

    protected void Start() {
        
    }

    protected void OnDestroy() {
        
    }

    public void OnPointerDown(PointerEventData eventData) {

    }


    
    //public void OnPointerDown(PointerEventData eventData) {
    //    if (selectObject == targetObject || selectObject != null)
    //        return;

    //    List<RaycastResult> result = new List<RaycastResult>();
    //    EventSystem.current.RaycastAll(eventData, result);

    //   if (result.Exists(x=>x.gameObject.name == "TargetScope")) {
    //        if(result.Exists(x=>x.gameObject.name == "TargetMask")) {

    //            if (targetObject.GetComponent<CardHandler>() != null) {
    //                selectObject = result.Find(x => x.gameObject.GetComponent<CardHandler>()).gameObject;
    //            }
    //            else if (targetObject.GetComponent<Button>() != null) {
    //                selectObject = result.Find(x => x.gameObject.GetComponent<Button>()).gameObject;

    //                if (selectObject != null)
    //                    selectObject.GetComponent<Button>().onClick.Invoke();

    //            }
    //        }
    //   }
    //}

}
