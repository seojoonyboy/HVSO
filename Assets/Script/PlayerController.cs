﻿using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] GameObject phaseManager;
    // Start is called before the first frame update
    IEnumerator Start() {
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
        yield return new WaitForSeconds(3.0f);
        CustomEvent.Trigger(phaseManager, "EndTurn");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
