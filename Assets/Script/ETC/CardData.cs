using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IngameEditor {
    public class CardData : MonoBehaviour {
        public dataModules.CollectionCard data;

        public void Clear() {
            data = new dataModules.CollectionCard();
            transform.Find("Image").GetComponent<Image>().sprite = null;
            int index = transform.GetSiblingIndex();
            transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = index + "번 라인";
        }
    }
}