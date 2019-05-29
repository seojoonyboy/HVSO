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

    public List<GameObject> GetAllFieldUnits() {
        List<GameObject> _units = new List<GameObject>();
        foreach(GameObject unit in units) {
            if(unit != null) _units.Add(unit);
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
        for (int i=0; i<2; i++) {
            if(units[row, i] != null) {
                _units.Add(units[row, i]);
            }
        }
        return _units;
    }

    public Pos GetMyRow(GameObject gameObject) {
        Pos pos = new Pos();
        pos.col = gameObject.GetComponent<PlaceMonster>().x;
        pos.row = gameObject.GetComponent<PlaceMonster>().y;

        return pos;
    }
}

public struct Pos {
    public int row;
    public int col;
}
