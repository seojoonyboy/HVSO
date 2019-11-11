using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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
    public static GameObject instantiate(string text, Type type, UnityAction function = null, System.Action function2 = null, string title = null) {
        if (type == Type.INSERT) {
			Logger.LogWarning("enum INSERT는 매개변수 하나 더 있습니다!");
            return null;
		}
        
        GameObject modal = Resources.Load("Prefabs/ModalCanvas", typeof(GameObject)) as GameObject;
        GameObject tmp = Instantiate(modal);
        tmp.GetComponent<Modal>().SetData(text, function, type, function2, title);
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
			Logger.LogWarning("enum YESNO 또는 CHECK는 매개변수를 줄여주십시오!");
			return null;
		}
		GameObject modal = Resources.Load("Prefabs/ModalCanvas", typeof(GameObject)) as GameObject;
        GameObject tmp = Instantiate(modal);
        tmp.GetComponent<Modal>().SetData(text, placeHolderText, value, function);
        return tmp;
	}

    public static GameObject instantiateReconnectModal() {
        GameObject modal = Resources.Load("Prefabs/ReconnectCanvas", typeof(GameObject)) as GameObject;
        return modal;
    }

    /// <summary>
    /// Modal 창 생성기 (YESNO와 CHECK 편)에 대한 데이터 세팅
    /// </summary>
    /// <param name="text">설명에 들어갈 내용</param>
	/// <param name="type">Modal.Type.종류</param>
	/// <param name="function">yes 버튼 누를 경우 실행 함수(필요하면)</param>
	/// <param name="title">제목에 들어갈 내용(필요하면)(급하게 넣은 매개변수)</param>
	public void SetData(string text, UnityAction function, Type type, System.Action function2 = null, string title = null) {
        Text describe = null;
        Button yesButton = null;
        if (type == Type.CHECK) {
            checkModal.SetActive(true);
            describe = checkModal.transform.Find("Describe").GetComponent<Text>();
            yesButton = checkModal.transform.Find("Buttons/YesButton").GetComponent<Button>();
		}
        else if(type == Type.YESNO) {
            YesNoModal.SetActive(true);
            describe = YesNoModal.transform.Find("Describe").GetComponent<Text>();
            yesButton = YesNoModal.transform.Find("Buttons/YesButton").GetComponent<Button>();
            if (function2 != null) {
                action = function2;
                EscapeKeyController.escapeKeyCtrl.AddEscape(function2);
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
		Destroy(gameObject);
	}

	private char MyValidate(char charToValidate) {
		if(charToValidate == ' ') charToValidate = '\0';
		return charToValidate;
	}
}
