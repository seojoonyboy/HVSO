using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadResourceController : MonoBehaviour
{

    public Vector2 startPos;
    public Vector2 startRandomPos;
    public Vector2 targetPos;



    void Update()
    {
        if(Vector2.Distance(transform.localPosition, targetPos) <= 0) {
            gameObject.SetActive(false);
            transform.localPosition = startPos;
        }
    }

    public void SetPositions(Vector2 startPos, Vector2 startRandomPos, Vector2 targetPos) {
        this.startPos = startPos;
        this.startRandomPos = startRandomPos;
        this.targetPos = targetPos;
    }
}
