using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuLockData", menuName = "MenuLockData", order = 0)]
public class MenuLockData : ScriptableObject {
    public List<string> lockMenuList;
    public List<string> unlockMenuList;
}
