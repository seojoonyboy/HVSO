using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PingpongTest : MonoBehaviour
{
    public GameObject testObject;
    public GameObject dummy;

    // Start is called before the first frame update
    void Start()
    {
        Color color = testObject.GetComponent<Image>().color;

        //iTween.ColorTo(testObject, testObject.GetComponent<Image>().color, 1.2f);
        //iTween.ColorTo(testObject, iTween.Hash("color", color, "a", 0, "time", 1.2f,"easeType", iTween.EaseType.easeInOutSine, "loopType", "pingPong"));
        //iTween.ValueTo(gameObject, iTween.Hash("from", 1f, "to", 0f,"time", 1.2f, "easeType", iTween.EaseType.easeInOutSine, "onupdate", "UpdateColor"));

        
        iTween.MoveTo(dummy, iTween.Hash("x", 0, "time", 1.5f, "easetype", iTween.EaseType.easeInCirc, "loopType", "pingPong", "onupdate", "UpdateColor"));

    }

    void UpdateColor() {
        Color color = testObject.GetComponent<Image>().color;
        color.a = dummy.transform.position.x;
        //color.a = val;
        testObject.GetComponent<Image>().color = color;
    }

   





    // Update is called once per frame
    void Update()
    {
        
    }
}
