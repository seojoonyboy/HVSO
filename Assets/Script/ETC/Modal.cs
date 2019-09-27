using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Modal : MonoBehaviour {
    [SerializeField] private GameObject InnerModal;
	[SerializeField] private Button yesButton;
	[SerializeField] private Button noButton;
	[SerializeField] private Button closeBtn;
	[SerializeField] private Text describe;
	[SerializeField] private GameObject inputGameObject;
	[SerializeField] private Text insertTitle;
	[SerializeField] private InputField inputField;
    const float DefaultHeight = 455f;
    const float DefaultWidth = 745f;
    const float InputHeight = 550f;

	public enum Type {
		YESNO,
		CHECK,
		INSERT
	}
	/// <summary>
	/// Modal 창 생성기 (YESNO와 CHECK 편)
	/// </summary>
	/// <param name="text">설명에 들어갈 내용</param>
	/// <param name="type">Modal.Type.종류</param>
	/// <param name="function">yes 버튼 누를 경우 실행 함수(필요하면)</param>
	/// <param name="title">제목에 들어갈 내용(필요하면)(급하게 넣은 매개변수)</param>
	public static void instantiate(string text, Type type, UnityAction function = null, string title = null) {
        if (type == Type.INSERT) {
			Logger.LogWarning("enum INSERT는 매개변수 하나 더 있습니다!");
			return;
		}
		GameObject modal = Resources.Load("Prefabs/ModalCanvas", typeof(GameObject)) as GameObject;
        Canvas canvas = (Canvas)FindObjectOfType(typeof(Canvas));
        if(canvas == null) {
            Logger.LogError("no Canvas");
            return;
        }
        modal.GetComponent<Modal>().InnerModal.GetComponent<RectTransform>().sizeDelta = new Vector2(DefaultWidth, DefaultHeight);
        Instantiate(modal).GetComponent<Modal>().SetData(text, function, type, title);
        //Instantiate(modal, canvas.transform, false).GetComponent<Modal>().setData(text, function, type);
	}
    /// <summary>
    /// Modal 창 생성기 (Insert 편) 
    /// </summary>
    /// <param name="text">제목 들어갈 내용</param>
    /// <param name="descText">inputField 빈공간에 들어갈 내용</param>
    /// <param name="inputText">inputField value</param>
    /// <param name="type">Modal.Type.종류</param>
    /// <param name="function">yes 버튼 누를 경우 실행 함수</param>
    public static GameObject instantiate(string text, string descText, string inputText, Type type, UnityAction<string> function) {
		if(type != Type.INSERT) {
			Logger.LogWarning("enum YESNO 또는 CHECK는 매개변수를 줄여주십시오!");
			return null;
		}
		GameObject modal = Resources.Load("Prefabs/ModalCanvas", typeof(GameObject)) as GameObject;
        Canvas canvas = (Canvas)FindObjectOfType(typeof(Canvas));
        if(canvas == null) {
            Logger.LogError("no Canvas");
            return null;
        }
        GameObject tmp = Instantiate(modal);
        tmp.GetComponent<Modal>().SetData(text, descText, inputText, function);
        tmp.GetComponent<Modal>().InnerModal.GetComponent<RectTransform>().sizeDelta = new Vector2(DefaultWidth, InputHeight);
        return tmp;
        //Instantiate(modal, canvas.transform, false).GetComponent<Modal>().setData(text, inputText, function);
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
	public void SetData(string text, UnityAction function, Type type, string title = null) {
		if(type == Type.CHECK) {
			noButton.gameObject.SetActive(false);
			yesButton.GetComponentInChildren<Text>().text = "확인";
		}
		insertTitle.text = title;
		describe.text = text;
		yesButton.onClick.AddListener(CloseButton);
		closeBtn.gameObject.SetActive(false);
		if(function == null) return;
		yesButton.onClick.AddListener(function);
	}

	public void SetData(string text, string descText, string inputText, UnityAction<string> function, bool closeExist = false) {
		describe.gameObject.SetActive(false);
		inputGameObject.SetActive(true);
		noButton.gameObject.SetActive(false);
		yesButton.GetComponentInChildren<Text>().text = "저장";
		insertTitle.text = text;
		inputField.GetComponentInChildren<Text>().text = descText;
        inputField.text = inputText;
		inputField.onValidateInput += delegate(string input, int charIndex, char addedChar) { return MyValidate(addedChar);};
		closeBtn.gameObject.SetActive(closeExist);
        yesButton.onClick.AddListener(() => {CheckInputText(inputField.text, function);});
	}

	private void CheckInputText(string text, UnityAction<string> function) {
		if(string.IsNullOrEmpty(text)) {
			//instantiate("내용이 비어있습니다\n내용을 채워주세요", Type.CHECK);
			Text warning = inputGameObject.transform.GetChild(1).GetComponent<Text>();
			warning.color = Color.red;
			warning.fontSize = 40;
			Handheld.Vibrate();
			return;
		}
		function(text);
		CloseButton();
	}

	private void Start() {
		noButton.onClick.AddListener(CloseButton);
		closeBtn.onClick.AddListener(CloseButton);
	}

	private void CloseButton() {
		Destroy(gameObject);
	}

	private char MyValidate(char charToValidate) {
		if(charToValidate == ' ') charToValidate = '\0';
		return charToValidate;
	}
}
