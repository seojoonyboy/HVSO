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

    public virtual bool CheckUnitPosition(int col, int row) {
        bool check = false;

        if (units[col, row] == null)
            check = true;
        else
            check = false;

        return check;
    }

    public virtual int CheckLineEmptyCount(int col) {
        int count = 0;

        for(int i = 0; i < 5; i++) {
            if (units[col, i] == null)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 자리 이동
    /// </summary>
    /// <param name="target">대상</param>
    /// <param name="prevPos">이전 위치</param>
    /// <param name="row">새로운 위치 row</param>
    /// <param name="col">새로운 위치 col</param>
    /// <returns></returns>
    IEnumerator UnitChangeCoroutine(GameObject target, Pos prevPos, int row, int col) {
        yield return new WaitForSeconds(1.0f);

        target.transform.SetParent(transform.GetChild(row).GetChild(col));
        target.transform.localPosition = Vector3.zero;

        target.GetComponent<PlaceMonster>().ChangePosition(
            col, 
            row, 
            transform.GetChild(row).GetChild(col).position
        );

        Logger.Log(string.Format("prev Pos Col : {0}",prevPos.col));
        Logger.Log(string.Format("prev Pos Row : {0}", prevPos.row));

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
        Debug.Log("라인 검사 : " + col);
        List<GameObject> _units = new List<GameObject>();
        for (int i = 0; i < 2; i++) {
            if (units[col, i] != null) {
                _units.Add(units[col, i]);
            }
        }
        Debug.Log("라인 검사 결과 : " + _units.Count);
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
