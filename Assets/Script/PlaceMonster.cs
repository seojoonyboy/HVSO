using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;


public class PlaceMonster : MonoBehaviour
{
    public Unit unit;

    private void Start()
    {
        unit.cost = 0;
        unit.HP = 1;
    }


}
