using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldUnitsObserver : SerializedMonoBehaviour {
    [TableMatrix(SquareCells = true)]
    public GameObject[,] units = new GameObject[5, 2];

    public void UnitAdded(GameObject target, int row, int col) {
        units[row, col] = target;
    }

    public void UnitRemoved(int row, int col) {
        units[row, col] = null;
    }

    public void UnitChangePosition(GameObject target, int row, int col) {
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
        StartCoroutine(UnitChangeCoroutine(target, prevPos, row, col));
    }

    IEnumerator UnitChangeCoroutine(GameObject target, Pos prevPos, int row, int col) {
        yield return new WaitForSeconds(1.0f);

        target.transform.SetParent(transform.GetChild(col).GetChild(row));
        target.transform.localPosition = Vector3.zero;
        target.GetComponent<PlaceMonster>().unitLocation = transform.GetChild(col).GetChild(row).position;
        units[prevPos.row, prevPos.col] = null;
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
    public List<GameObject> GetAllFieldUnits(int row) {
        List<GameObject> _units = new List<GameObject>();
        for (int i = 0; i < 2; i++) {
            if (units[row, i] != null) {
                _units.Add(units[row, i]);
            }
        }
        return _units;
    }

    public Pos GetMyPos(GameObject gameObject) {
        Pos pos = new Pos();
        pos.row = gameObject.GetComponent<PlaceMonster>().x;
        pos.col = gameObject.GetComponent<PlaceMonster>().y;

        return pos;
    }
}

public struct Pos {
    public int row;
    public int col;
}
