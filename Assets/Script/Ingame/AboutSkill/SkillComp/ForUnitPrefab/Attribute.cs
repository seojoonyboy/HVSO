using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attribute : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        Init();
    }

    // Update is called once per frame
    void Update() {

    }

    public virtual void Init() {

    }

    /// <summary>
    /// 스킬 누적 처리
    /// </summary>
    public virtual void Accumulate() {

    }

    /// <summary>
    /// 스킬 차감 처리
    /// </summary>
    public virtual void Subtraction() {

    }
}
