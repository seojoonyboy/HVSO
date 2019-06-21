using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldUnitsObserver : SerializedMonoBehaviour {
    [TableMatrix(SquareCells = true)]
    public GameObject[,] units = new GameObject[5, 2];

    public void UnitAdded(GameObject target, int col, int row) {
        units[col, row] = target;
    }

    public void UnitRemoved(int col, int row) {
        units[col, row] = null;
    }

    public virtual void UnitChangePosition(GameObject target, int col, int row) {
        Logger.Log("Row : " + row);
        Logger.Log("Col : " + col);

        Pos prevPos = GetMyPos(target);
        units[col, row] = target;

        Vector2 targetPos = transform.GetChild(row).GetChild(col).position;
        iTween.MoveTo(
            target,
            new Vector2(targetPos.x, targetPos.y),
            1.0f
        );
        StartCoroutine(UnitChangeCoroutine(target, prevPos, row, col));
    }

    IEnumerator UnitChangeCoroutine(GameObject target, Pos prevPos, int row, int col) {
        yield return new WaitForSeconds(1.0f);

        target.transform.SetParent(transform.GetChild(row).GetChild(col));
        target.transform.localPosition = Vector3.zero;

        target.GetComponent<PlaceMonster>().ChangePosition(
            row, 
            col, 
            transform.GetChild(row).GetChild(col).position
        );

        Logger.Log(prevPos.col);
        Logger.Log(prevPos.row);

        units[prevPos.col, prevPos.row] = null;
        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, null, null);
    }

    public List<GameObject> GetAllFieldUnits() {
        List<GameObject> _units = new List<GameObject>();
        foreach (GameObject unit in units) {
            if (unit != null) _units.Add(unit);
        }
        return _units;
    }

    /// <summary>
    /// 한줄 반환
    /// </summary>
    /// <param name="row">row</param>
    /// <returns></returns>
    public List<GameObject> GetAllFieldUnits(int col) {
        List<GameObject> _units = new List<GameObject>();
        for (int i = 0; i < 2; i++) {
            if (units[col, i] != null) {
                _units.Add(units[col, i]);
            }
        }
        return _units;
    }

    public virtual Pos GetMyPos(GameObject gameObject) {
        Pos pos = new Pos();
        pos.col = gameObject.GetComponent<PlaceMonster>().x;
        pos.row = gameObject.GetComponent<PlaceMonster>().y;

        return pos;
    }

    public void RefreshFields(Transform[][] arr) {
        int colCnt = 0;
        foreach(Transform[] col in arr) {
            int rowCnt = 0;
            foreach(Transform row in col) {
                if(row.transform.childCount != 0) {
                    units[colCnt, rowCnt] = row.transform.GetChild(0).gameObject;
                }
                else {
                    units[colCnt, rowCnt] = null;
                }
                rowCnt++;
            }
            colCnt++;
        }
    }
}

public struct Pos {
    public int row;
    public int col;
}
