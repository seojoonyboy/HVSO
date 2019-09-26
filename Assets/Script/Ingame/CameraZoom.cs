using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public Transform target;
    public Camera camera;

    public void Awake() {
        camera = gameObject.GetComponent<Camera>();
    }

    public void UpdateSize(float num) {
        camera.orthographicSize = num;
    }

}
