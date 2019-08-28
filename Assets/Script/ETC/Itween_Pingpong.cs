using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Itween_Pingpong : MonoBehaviour {
    public float delay = 0.1f;
    public float moveAmount = 2;
    public float speed = 5.0f;

    Vector3 origin;
    void Awake() {
        origin = transform.localPosition;
    }

    void OnEnable() {
        transform.localPosition = origin;
        iTween.Stop(gameObject);
        iTween
            .MoveBy(
            gameObject, 
            iTween.Hash(
                "y", moveAmount, 
                "speed", speed,
                "easeType", iTween.EaseType.easeInOutSine,
                "loopType", "pingPong", 
                "delay", delay)
            );
    }
}
