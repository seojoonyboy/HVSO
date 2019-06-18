using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManagement : MonoBehaviour
{
    public static DebugManagement instance { get; private set; }

    public DebugPlayer player, enemyPlayer;
    public SpineEffectManager spineEffectManager;
    public Camera ingameCamera;
    public Vector3 cameraPos;
    public GameObject unitDeadObject;
    public Dropdown idDropDown;

    public GameObject UnitCard;
    public GameObject MagicCard;

    private void Awake() {
        instance = this;
        Dictionary<string, CardData> cardData = transform.GetComponent<DebugData>().cardData;
        List<string> keyList = new List<string>(cardData.Keys);
        idDropDown.AddOptions(keyList);
    }

    private void Start() {
        SetCamera();
    }

    public void SetCamera() {
        cameraPos = Camera.main.transform.position;
    }

    public IEnumerator cameraShake(float time, int power) {
        float timer = 0;
        float cameraSize = ingameCamera.orthographicSize;
        while (timer <= time) {


            ingameCamera.transform.position = (Vector3)Random.insideUnitCircle * 0.1f * power + cameraPos;

            timer += Time.deltaTime;
            yield return null;
        }
        ingameCamera.orthographicSize = cameraSize;
        ingameCamera.transform.position = cameraPos;
    }


    public void GenerateCard() {


    }
}
