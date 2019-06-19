using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DebugFieldObserver : FieldUnitsObserver {

    public override void UnitChangePosition(GameObject target, int row, int col) {
        Pos prevPos = GetMyPos(target);
        units[row, col] = target;

        //Debug.Log("Row : " + row);
        //Debug.Log("Col : " + col);
        Vector2 targetPos = transform.GetChild(col).GetChild(row).position;
        iTween.MoveTo(
            target,
            new Vector2(targetPos.x, targetPos.y),
            1.0f
        );
        StartCoroutine(UnitDebugChangeCoroutine(target, prevPos, row, col));
    }

    IEnumerator UnitDebugChangeCoroutine(GameObject target, Pos prevPos, int row, int col) {
        yield return new WaitForSeconds(1.0f);

        target.transform.SetParent(transform.GetChild(col).GetChild(row));
        target.transform.localPosition = Vector3.zero;

        target.GetComponent<DebugUnit>().ChangePosition(
            row,
            col,
            transform.GetChild(col).GetChild(row).position
        );
        units[prevPos.row, prevPos.col] = null;
    }

    public override Pos GetMyPos(GameObject gameObject) {
        Pos pos = new Pos();
        pos.row = gameObject.GetComponent<DebugUnit>().x;
        pos.col = gameObject.GetComponent<DebugUnit>().y;

        return pos;
    }
}
