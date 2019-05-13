using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;
using TMPro;


public class PlaceMonster : MonoBehaviour
{
    public Unit unit;

    private void Start()
    {
        transform.Find("HP").GetComponentInChildren<TextMeshPro>().text = unit.HP.ToString();
        transform.Find("ATK").GetComponentInChildren<TextMeshPro>().text = unit.power.ToString();

        PlayMangement.instance.player.placement[0, 1] = 1;

    }


}
