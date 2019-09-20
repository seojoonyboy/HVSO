using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeaderController : MonoBehaviour {
    [SerializeField] GameObject ShopPanel, OptionPanel, MyFriendPanel, NotifyPanel;
    // Start is called before the first frame update
    void Start() {
        SetMyResource();
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnOptionBtnClicked() {
        OptionPanel.SetActive(true);
    }

    public void OnFriendBtnClicked() {
        MyFriendPanel.SetActive(true);
    }

    public void OnShopBtnClicked() {
        ShopPanel.SetActive(true);
    }

    public void SetMyResource() {

    }

    public void ToMainScene() {
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }
}
