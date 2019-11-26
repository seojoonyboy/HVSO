using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LeagueData", menuName = "LeagueData", order = 0)]
public class LeagueData : ScriptableObject {
    public string prevRank, newRank;    //인게임 전 랭크, 인게임 후 랭크
    public int prevMMR, newMMR;         //인게임 전 MMR, 인게임 후 MMR
    public int prevMaxMMR;              //이전 등급 최댓값
}