using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManagement : MonoBehaviour
{
    public static DebugManagement instance { get; private set; }

    public DebugPlayer player, enemyPlayer;
    public SpineEffectManager spineEffectManager;
    public Camera ingameCamera;
    public Vector3 cameraPos;
    public GameObject unitDeadObject;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        SetCamera();
    }

    public void SetCamera() {
        cameraPos = Camera.main.transform.position;
    }

    public IEnumerator cameraShake(float time) {
        float timer = 0;
        float cameraSize = ingameCamera.orthographicSize;
        while (timer <= time) {


            ingameCamera.transform.position = (Vector3)Random.insideUnitCircle * 0.1f + cameraPos;

            timer += Time.deltaTime;
            yield return null;
        }
        ingameCamera.orthographicSize = cameraSize;
        ingameCamera.transform.position = cameraPos;
    }
}
