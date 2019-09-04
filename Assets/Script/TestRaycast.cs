using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestRaycast : MonoBehaviour , IPointerClickHandler 
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {        
    }

    public void OnPointerClick(PointerEventData eventData) {

        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, result);

        if (result.Exists(x => x.gameObject.name == "TargetScope")) {
            if (result.Exists(x => x.gameObject.name == "TargetMask")) {
                Debug.Log("통과!");
            }
            else
                Debug.Log("하나!");            
        }


    }



}
