using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManagement : PlayMangement
{
    public static DebugManagement Instance { get; private set; }

    
    public DebugPlayer player, enemyPlayer;
    public SpineEffectManager spineEffectManager;
    public Camera ingameCamera;
    public Vector3 cameraPos;
    public GameObject unitDeadObject;
    public Dropdown idDropDown;
    public GameObject backGround;

    public GameObject UnitCard;
    public GameObject MagicCard;

    public bool isGame = true;
    public bool isMulligan = true;
    public bool infoOn = false;
    public static bool dragable = true;

    public Transform cardDragCanvas;
    public Transform cardInfoCanvas;

    public ResourceManager resource;

    private void Awake() {
        Instance = this;
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

    public enum LineState {
        hill,
        flat,
        forest,
        water
    }

    public void DrawCard() {
        if (isGame == false) return;
        bool race = player.isHuman;
        string text = idDropDown.options[idDropDown.value].text;
        CardData card = DebugData.Instance.cardData[text];

        player.cdpm.AddCard(null, card);
    }


    [SerializeField] FieldUnitsObserver playerUnitsObserver;
    public FieldUnitsObserver PlayerUnitsObserver {
        get {
            return playerUnitsObserver;
        }
    }

    [SerializeField] FieldUnitsObserver enemyUnitsObserver;
    public FieldUnitsObserver EnemyUnitsObserver {
        get {
            return enemyUnitsObserver;
        }
    }
}
