using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LeagueData", menuName = "LeagueData", order = 0)]
public class LeagueData : ScriptableObject {
    public AccountManager.LeagueInfo prevLeagueInfo;
    public AccountManager.LeagueInfo leagueInfo;
}