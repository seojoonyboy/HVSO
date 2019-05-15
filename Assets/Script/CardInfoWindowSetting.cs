using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfoWindowSetting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ExitClassInfo() {
        gameObject.SetActive(false);
    }
}
