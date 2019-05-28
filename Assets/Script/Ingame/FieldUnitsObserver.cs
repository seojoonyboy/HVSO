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
}
