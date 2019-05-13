using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AccountManager : Singleton<AccountManager> {
    protected AccountManager() { }
    public string DEVICEID { get; private set; }
    private StringBuilder sb = new StringBuilder();
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
