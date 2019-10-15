using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuExecute : MonoBehaviour {
    public MenuExecuteHandler handler;
    public List<string> args;

    public virtual void Initialize(List<string> args) {
        this.args = args;
        handler = GetComponent<MenuExecuteHandler>();
    }

    public virtual void Execute() { }
}
