using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour {
    [SerializeField] Text _text;
    float timer = 0;
    int min = 0;

    void Start() {
        _text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {
        timer += Time.deltaTime;
        if (timer >= 60) {
            min++;
            timer -= 60;
        }

        _text.text = string.Format("{0:D2} : {1:D2}", min, (int)timer);
    }

    public void ResetTimer() {
        timer = 0;
    }
}
