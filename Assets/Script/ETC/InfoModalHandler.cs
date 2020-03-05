namespace ButtonModules {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using UniRx;
    using System;
    using Sirenix.OdinInspector;

    public class InfoModalHandler : MonoBehaviour {
        [SerializeField] TextMeshProUGUI headerTmPro, contentsTmPro;
        [SerializeField] [MultiLineProperty(10)] string header, content;
        [SerializeField] bool needTranslate;

        public void OnPanel(string header, string content) {
            this.header = header;
            this.content = content;

            gameObject.SetActive(true);
        }

        public void OffPanel() {
            gameObject.SetActive(false);
        }

        public void OnClick() {
            if (needTranslate) { }
            gameObject.SetActive(true);

            headerTmPro.text = header;
            contentsTmPro.text = content;

            IObservable<long> clickStream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
            clickStream.Subscribe(x => OffPanel());
        }
    }
}
