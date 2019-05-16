using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfoWindowSetting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    public void ExitClassInfo() {
        gameObject.SetActive(false);
    }
}
