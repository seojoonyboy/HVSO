using UnityEngine;
using UnityEngine.UI;

public class HighlightPingpong : MonoBehaviour
{    
    public float time = 1.5f;
    public Image glowObject;
    [HideInInspector]
    public bool active = false;

    public void StartPingPong() {
        gameObject.transform.position = new Vector3(1, 0, 0);
        iTween.MoveTo(gameObject, iTween.Hash("x", 0, "easetype", iTween.EaseType.easeInOutElastic, "loopType", "pingPong", "onupdate", "ColorUpdate"));

    }

    public void ColorUpdate() {
        Color color = glowObject.color;
        color.a = gameObject.transform.position.x;
        glowObject.color = color;
    }

    public void EndPingPong() {
        iTween.Stop(gameObject);
    }
}
