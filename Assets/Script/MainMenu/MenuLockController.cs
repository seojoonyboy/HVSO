using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLockController : SerializedMonoBehaviour {
    [SerializeField] Dictionary<string, GameObject> menues;

    public void Unlock(string keyword) {

    }

    public void Lock(string keyword) {

    }

    public GameObject FindMenuObject(string keyword) {
        switch (keyword) {
            case "스토리":
                break;
            case "카드":
                break;
            case "배틀":
                break;
        }
        return null;
    }
}
