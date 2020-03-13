using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpMenuController : MonoBehaviour {

    bool opened = false;
    void Update() {
        if (Input.GetMouseButtonDown(0) && opened) {
            if(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null) {
                StartCoroutine(CloseList());
            }
        }
    }

    public void ActivateList() {
        gameObject.SetActive(true);
        opened = true;
    }

    IEnumerator CloseList() {
        opened = false;
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
}
