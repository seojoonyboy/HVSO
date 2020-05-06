using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace hvso {
    public class VersionCheck : MonoBehaviour
    {
        void Start() {
            GetComponent<Text>().text += Application.version;
        }
    }
}
