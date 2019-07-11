using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DebugFieldObserver : FieldUnitsObserver {

    public override void UnitChangePosition(GameObject target, Pos pos, bool isHuman) {
        GameObject[,] units = null;
        if (isHuman) units = humanUnits;
        else units = orcUnits;

        Pos prevPos = GetMyPos(target);
        units[pos.row, pos.col] = target;

        //Debug.Log("Row : " + row);
        //Debug.Log("Col : " + col);
        Vector2 targetPos = transform.GetChild(pos.col).GetChild(pos.row).position;
        iTween.MoveTo(
            target,
            new Vector2(targetPos.x, targetPos.y),
            1.0f
        );
        StartCoroutine(UnitDebugChangeCoroutine(target, prevPos, pos, isHuman));
    }

    IEnumerator UnitDebugChangeCoroutine(GameObject target, Pos prevPos, Pos newPos, bool isHuman) {
        yield return new WaitForSeconds(1.0f);

        target.transform.SetParent(transform.GetChild(newPos.col).GetChild(newPos.row));
        target.transform.localPosition = Vector3.zero;

        target.GetComponent<DebugUnit>().ChangePosition(
            newPos.row,
            newPos.col,
            transform.GetChild(newPos.col).GetChild(newPos.row).position
        );

        GameObject[,] units = null;
        if (isHuman) units = humanUnits;
        else units = orcUnits;

        units[prevPos.row, prevPos.col] = null;
    }

    public override Pos GetMyPos(GameObject gameObject) {
        Pos pos = new Pos();
        pos.row = gameObject.GetComponent<DebugUnit>().x;
        pos.col = gameObject.GetComponent<DebugUnit>().y;

        return pos;
    }
}
