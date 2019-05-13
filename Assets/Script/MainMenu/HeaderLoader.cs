using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeaderLoader : MonoBehaviour {
    [SerializeField] GameObject header;
    // Start is called before the first frame update
    void Start() {
        GameObject _header = Instantiate(header, transform);
        _header.transform.SetAsLastSibling();
    }

    // Update is called once per frame
    void Update() {

    }
}
