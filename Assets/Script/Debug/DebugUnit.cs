using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugUnit : PlaceMonster
{
    List<Buff> buffList = new List<Buff>();

    void OnDestroy() {
    }

    private void Start() {
    }


    private void SkipAttack() {
        atkCount = 0;
    }

    private void ReturnPosition() {
        unitSpine.transform.GetComponent<MeshRenderer>().sortingOrder = 50;
        iTween.MoveTo(gameObject, iTween.Hash("x", unitLocation.x, "y", unitLocation.y, "z", unitLocation.z, "time", 0.3f, "delay", 0.5f, "easetype", iTween.EaseType.easeInOutExpo));

    }
}
