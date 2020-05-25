using System.Collections;
using System.Collections.Generic;
using SocketFormat;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class Modal : MonoBehaviour {
    [SerializeField] private GameObject InnerModal;
    [SerializeField] GameObject checkModal, YesNoModal, InsertModal;
    System.Action action;

	public enum Type {
		YESNO,
		CHECK,
		INSERT
	}

    private void OnDestroy() {
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(action);
    }
    /// <summary>
    /// Modal 창 생성기 (YESNO와 CHECK 편)
    /// </summary>
    /// <param name="text">설명에 들어갈 내용</param>
    /// <param name="type">Modal.Type.종류</param>
    /// <param name="function">yes 버튼 누를 경우 실행 함수(필요하면)</param>
    /// <param name="title">제목에 들어갈 내용(필요하면)(급하게 넣은 매개변수)</param>
    public static GameObject instantiate(string text, Type type, UnityAction function = null, System.Action function2 = null, string title = null, string[] btnTexts = null, string headerText = null) {
        if (type == Type.INSERT) {
			//Logger.LogWarning("enum INSERT는 매개변수 하나 더 있습니다!");
            return null;
		}
        
        GameObject modal = Resources.Load("Prefabs/ModalCanvas", typeof(GameObject)) as GameObject;
        GameObject tmp = Instantiate(modal);
        tmp.GetComponent<Modal>().SetData(text, function, type, function2, title, btnTexts, headerText);
        return tmp;
	}
    /// <summary>
    /// Modal 창 생성기 (Insert 편) 
    /// </summary>
    /// <param name="text">제목 들어갈 내용</param>
    /// <param name="descText">inputField 빈공간에 들어갈 내용</param>
    /// <param name="inputText">inputField value</param>
    /// <param name="type">Modal.Type.종류</param>
    /// <param name="function">yes 버튼 누를 경우 실행 함수</param>
    public static GameObject instantiate(string text, string placeHolderText, string value, Type type, UnityAction<string> function) {
		if(type != Type.INSERT) {
			//Logger.LogWarning("enum YESNO 또는 CHECK는 매개변수를 줄여주십시오!");
			return null;
		}
		GameObject modal = Resources.Load("Prefabs/ModalCanvas", typeof(GameObject)) as GameObject;
        GameObject tmp = Instantiate(modal);
        tmp.GetComponent<Modal>().SetData(text, placeHolderText, value, function);
        return tmp;
	}

    /// <summary>
    /// 자동으로 사라지는 모달 (버튼 없음)
    /// </summary>
    /// <param name="message">내용</param>
    /// <param name="headerText">제목</param>
    /// <param name="waitTime">사라지기까지 대기 시단 (초)</param>
    /// <returns></returns>
    public static GameObject instantiateAutoHideModal(string message, float waitTime = 3.0f) {
        Logger.Log("<color=red>" + message + "</color>");
        GameObject modal = Resources.Load("Prefabs/TimerModalCanvas", typeof(GameObject)) as GameObject;
        GameObject tmp = Instantiate(modal);
        tmp.transform.Find("ModalWindow/CheckModalA").gameObject.SetActive(true);
        tmp.transform.Find("ModalWindow/CheckModalA/Describe").GetComponent<TextMeshProUGUI>().text = message;
        tmp.GetComponent<DestroyTimer>().StartTimer(waitTime);
        return tmp;
    }

    /// <summary>
    /// 연결이 끊어졌습니다. 재접속을 시도중입니다... 에 대한 모달
    /// </summary>
    /// <param name="message"></param>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    public static GameObject instantiateOpponentWaitingModal(string message, float waitTime = 30f) {
        Logger.Log("<color=red>" + message + "</color>");
        GameObject modal = Resources.Load("Prefabs/TimerModalCanvas", typeof(GameObject)) as GameObject;
        GameObject tmp = Instantiate(modal);
        tmp.transform.Find("ModalWindow/CheckModalB").gameObject.SetActive(true);
        tmp.transform.Find("ModalWindow/CheckModalB/Describe").GetComponent<TextMeshProUGUI>().text = message;
        TextMeshProUGUI textComp = tmp.transform.Find("ModalWindow/CheckModalB/TimerText").GetComponent<TextMeshProUGUI>();
        tmp.GetComponent<DestroyTimer>().StartTimer(waitTime, textComp);
        return tmp;
    }
    
    /// <summary>
    /// 상대 재접속중... 에 대한 모달
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static GameObject instantiateOpponentWaitingFinalModal(string message) {
        Logger.Log("<color=red>" + message + "</color>");
        GameObject modal = Resources.Load("Prefabs/TimerModalCanvas", typeof(GameObject)) as GameObject;
        GameObject tmp = Instantiate(modal);
        tmp.transform.Find("ModalWindow/CheckModalC").gameObject.SetActive(true);
        tmp.transform.Find("ModalWindow/CheckModalC/Describe").GetComponent<TextMeshProUGUI>().text = message;
        Destroy(tmp.GetComponent<DestroyTimer>());
        return tmp;
    }

    /// <summary>
    /// wifi 로고 모달
    /// </summary>
    /// <returns></returns>
    public static GameObject instantiateReconnectModal() {
        GameObject modal = Resources.Load("Prefabs/ReconnectCanvas", typeof(GameObject)) as GameObject;
        modal.name = "ReconnectCanvas";
        return modal;
    }

    /// <summary>
    /// 최종적으로 연결 실패한 경우
    /// </summary>
    /// <returns></returns>
    public static GameObject instantiateReconnectFailModal(string message, string btnTxt) {
        GameObject modal = Resources.Load("Prefabs/ReconnectFailureCanvas", typeof(GameObject)) as GameObject;
        modal.transform.Find("ModalWindow/Message").GetComponent<TextMeshProUGUI>().text = message;
        modal.transform.Find("ModalWindow/Button/Text").GetComponent<TextMeshProUGUI>().text = btnTxt;
        return modal;
    }

    /// <summary>
    /// Modal 창 생성기 (YESNO와 CHECK 편)에 대한 데이터 세팅
    /// </summary>
    /// <param name="text">설명에 들어갈 내용</param>
	/// <param name="type">Modal.Type.종류</param>
	/// <param name="function">yes 버튼 누를 경우 실행 함수(필요하면)</param>
	/// <param name="title">제목에 들어갈 내용(필요하면)(급하게 넣은 매개변수)</param>
	public void SetData(string text, UnityAction function, Type type, System.Action function2 = null, string title = null, string[] btnTexts = null, string headerText = null) {
        TextMeshProUGUI describe = null;
        Button yesButton = null;
        if (type == Type.CHECK) {
            checkModal.SetActive(true);
            describe = checkModal.transform.Find("Describe").GetComponent<TextMeshProUGUI>();
            yesButton = checkModal.transform.Find("Buttons/YesButton").GetComponent<Button>();

            if(btnTexts != null && btnTexts.Length >= 1) {
                yesButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = btnTexts[0];
            }

            if (!string.IsNullOrEmpty(headerText)) {
                checkModal.transform.Find("Title/Text").GetComponent<TextMeshProUGUI>().text = headerText;
            }
        }
        else if(type == Type.YESNO) {
            YesNoModal.SetActive(true);
            describe = YesNoModal.transform.Find("Describe").GetComponent<TextMeshProUGUI>();
            yesButton = YesNoModal.transform.Find("Buttons/YesButton").GetComponent<Button>();
            if (function2 != null) {
                action = function2;
                EscapeKeyController.escapeKeyCtrl.AddEscape(function2);
            }

            if (btnTexts != null && btnTexts.Length >= 2) {
                yesButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = btnTexts[0];
                YesNoModal.transform.Find("Buttons/NoButton/Text").GetComponent<TextMeshProUGUI>().text = btnTexts[1];
            }
            else {
                string[] response = new string[2];
                response[0] = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_yes");
                response[1] = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_no");

                yesButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = response[0];
                YesNoModal.transform.Find("Buttons/NoButton/Text").GetComponent<TextMeshProUGUI>().text = response[1];
            }

            if (!string.IsNullOrEmpty(headerText)) {
                YesNoModal.transform.Find("Title/Text").GetComponent<TextMeshProUGUI>().text = headerText;
            }
        }
        else { Logger.LogError("Modal 타입이 올바르지 않습니다!"); }

        if(describe != null) {
            describe.text = text;
        }

        if(yesButton != null) {
            yesButton.onClick.AddListener(CloseButton);
            if(function != null) yesButton.onClick.AddListener(function);
        }
	}

    /// <summary>
    /// Modal 창 생성기 (Insert 편)에 대한 데이터 세팅
    /// </summary>
    /// <param name="text"></param>
    /// <param name="descText"></param>
    /// <param name="inputText"></param>
    /// <param name="function"></param>
    /// <param name="closeExist"></param>
	public void SetData(string text, string placeHolderText, string value, UnityAction<string> function, bool closeExist = false) {
        InsertModal.SetActive(true);
        Text describe = InsertModal.transform.Find("Describe").GetComponent<Text>();
        Text placeHolder = InsertModal.transform.Find("InputField").GetComponent<InputField>().placeholder.GetComponent<Text>();
        InputField inputValue = InsertModal.transform.Find("InputField").GetComponent<InputField>();

        describe.text = text;
        placeHolder.text = placeHolderText;
        inputValue.text = value;

        Button yesButton = InsertModal.transform.Find("Buttons/YesButton").GetComponent<Button>();
        yesButton.onClick.AddListener(() => { CheckInputText(inputValue.text, function); });
	}

	private void CheckInputText(string text, UnityAction<string> function) {
        if (string.IsNullOrEmpty(text)) {
            //instantiate("내용이 비어있습니다\n내용을 채워주세요", Type.CHECK);
            Text warning = InsertModal.transform.Find("Describe").GetComponent<Text>();
            warning.color = Color.red;
            warning.fontSize = 40;
            Handheld.Vibrate();
            return;
        }
        function(text);
        CloseButton();
    }

	public void CloseButton() {
		SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
        if (action != null) action();
        else Destroy(gameObject);        
	}

	private char MyValidate(char charToValidate) {
		if(charToValidate == ' ') charToValidate = '\0';
		return charToValidate;
	}
}
