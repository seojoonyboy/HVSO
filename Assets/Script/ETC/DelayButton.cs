using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DelayButton : Button {
    public float _delay = 1f;
    WaitForSeconds _delayTimer;

    public override void OnPointerClick(PointerEventData eventData) {
        StartCoroutine(DelayedClickRoutine());
    }

    protected override void Start() {
        base.Start();
        _delayTimer = new WaitForSeconds(_delay);
    }

    private IEnumerator DelayedClickRoutine() {
        // wait the delay time, then invoke the event
        yield return _delayTimer;
        onClick.Invoke();
    }
}
