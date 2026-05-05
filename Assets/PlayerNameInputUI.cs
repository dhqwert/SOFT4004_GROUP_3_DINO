using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup nhập tên player. Tự build UI runtime, bắt event OK/Hủy.
/// - Tự bật khi vào game lần đầu (player chưa từng nhập tên).
/// - Có thể gọi <see cref="ShowEditDialog"/> từ nút "Đổi tên".
/// </summary>
public class PlayerNameInputUI : MonoBehaviour
{
    public const string HAS_CUSTOM_NAME_KEY = "HasCustomPlayerName";

    public static PlayerNameInputUI instance;

    Canvas canvas;
    TMP_InputField inputField;
    TextMeshProUGUI titleText;
    TextMeshProUGUI errorText;
    Action<string> currentCallback;
    bool isFirstTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureSpawned()
    {
        if (instance != null) return;
        GameObject go = new GameObject("PlayerNameInputUI");
        DontDestroyOnLoad(go);
        go.AddComponent<PlayerNameInputUI>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.4f);
        if (PlayerPrefs.GetInt(HAS_CUSTOM_NAME_KEY, 0) == 0)
        {
            isFirstTime = true;
            ShowFirstTimeDialog();
        }
    }

    public void ShowFirstTimeDialog()
    {
        BuildUIIfNeeded();
        isFirstTime = true;
        titleText.text = "SET YOUR NAME";
        errorText.text = "";
        inputField.text = "";
        currentCallback = null;
        canvas.gameObject.SetActive(true);
    }

    public void ShowEditDialog(Action<string> onApplied = null)
    {
        BuildUIIfNeeded();
        isFirstTime = false;
        titleText.text = "CHANGE DISPLAY NAME";
        errorText.text = "";
        FirebaseLeaderboardService firebase = FirebaseLeaderboardService.instance;
        inputField.text = firebase != null ? firebase.LocalPlayerName : "";
        currentCallback = onApplied;
        canvas.gameObject.SetActive(true);
    }

    void OnConfirmClicked()
    {
        string name = inputField.text != null ? inputField.text.Trim() : "";
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
        {
            errorText.text = "Name must be at least 2 characters.";
            return;
        }
        if (name.Length > 14)
        {
            name = name.Substring(0, 14);
        }

        FirebaseLeaderboardService firebase = FirebaseLeaderboardService.instance;
        if (firebase != null)
        {
            firebase.SetLocalPlayerName(name);
            if (firebase.IsConfigured)
            {
                firebase.SyncLocalPlayerName(null);
            }
        }
        else
        {
            PlayerPrefs.SetString("PlayerName", name);
        }
        PlayerPrefs.SetInt(HAS_CUSTOM_NAME_KEY, 1);
        PlayerPrefs.Save();

        if (currentCallback != null) currentCallback.Invoke(name);
        currentCallback = null;
        canvas.gameObject.SetActive(false);
    }

    void OnCancelClicked()
    {
        if (isFirstTime)
        {
            errorText.text = "Please enter a name to continue.";
            return;
        }
        canvas.gameObject.SetActive(false);
    }

    void BuildUIIfNeeded()
    {
        if (canvas != null) return;

        GameObject canvasGo = new GameObject("PlayerNameCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(canvasGo);
        canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 1f;

        // Backdrop
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        backdrop.transform.SetParent(canvasGo.transform, false);
        RectTransform brt = (RectTransform)backdrop.transform;
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        // Panel
        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGo.transform, false);
        RectTransform prt = (RectTransform)panel.transform;
        prt.anchorMin = new Vector2(0.5f, 0.5f); prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.pivot = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(820f, 760f);
        panel.GetComponent<Image>().color = new Color(0.08f, 0.16f, 0.42f, 1f);

        TextMeshProUGUI title = CreateText(panel.transform, "Title", "SET YOUR NAME", 60f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform titleRT = (RectTransform)title.transform;
        titleRT.anchorMin = new Vector2(0f, 1f); titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.sizeDelta = new Vector2(0f, 100f);
        titleRT.anchoredPosition = new Vector2(0f, -50f);
        title.color = new Color(1f, 0.85f, 0.25f, 1f);
        titleText = title;

        // Input field
        GameObject inputGo = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        inputGo.transform.SetParent(panel.transform, false);
        RectTransform inRT = (RectTransform)inputGo.transform;
        inRT.anchorMin = new Vector2(0.5f, 0.5f); inRT.anchorMax = new Vector2(0.5f, 0.5f);
        inRT.pivot = new Vector2(0.5f, 0.5f);
        inRT.sizeDelta = new Vector2(680f, 130f);
        inRT.anchoredPosition = new Vector2(0f, 80f);
        Image inputImg = inputGo.GetComponent<Image>();
        inputImg.color = new Color(1f, 1f, 1f, 0.95f);

        GameObject textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(inputGo.transform, false);
        RectTransform taRT = (RectTransform)textArea.transform;
        taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(20f, 8f); taRT.offsetMax = new Vector2(-20f, -8f);

        TextMeshProUGUI placeholder = CreateText(textArea.transform, "Placeholder", "Enter name...", 48f, FontStyles.Italic, TextAlignmentOptions.Left);
        RectTransform plRT = (RectTransform)placeholder.transform;
        plRT.anchorMin = Vector2.zero; plRT.anchorMax = Vector2.one;
        plRT.offsetMin = Vector2.zero; plRT.offsetMax = Vector2.zero;
        placeholder.color = new Color(0.4f, 0.4f, 0.4f, 1f);

        TextMeshProUGUI textComp = CreateText(textArea.transform, "Text", "", 48f, FontStyles.Bold, TextAlignmentOptions.Left);
        RectTransform tRT = (RectTransform)textComp.transform;
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = Vector2.zero; tRT.offsetMax = Vector2.zero;
        textComp.color = new Color(0.05f, 0.05f, 0.10f, 1f);

        TMP_InputField input = inputGo.GetComponent<TMP_InputField>();
        input.textViewport = taRT;
        input.textComponent = textComp;
        input.placeholder = placeholder;
        input.characterLimit = 14;
        inputField = input;

        // Error text
        TextMeshProUGUI err = CreateText(panel.transform, "ErrorText", "", 30f, FontStyles.Italic, TextAlignmentOptions.Center);
        RectTransform eRT = (RectTransform)err.transform;
        eRT.anchorMin = new Vector2(0f, 0.5f); eRT.anchorMax = new Vector2(1f, 0.5f);
        eRT.pivot = new Vector2(0.5f, 0.5f);
        eRT.sizeDelta = new Vector2(0f, 60f);
        eRT.anchoredPosition = new Vector2(0f, -10f);
        err.color = new Color(1f, 0.4f, 0.4f, 1f);
        errorText = err;

        // OK button
        GameObject ok = new GameObject("OK", typeof(RectTransform), typeof(Image), typeof(Button));
        ok.transform.SetParent(panel.transform, false);
        RectTransform okRT = (RectTransform)ok.transform;
        okRT.anchorMin = new Vector2(0.5f, 0f); okRT.anchorMax = new Vector2(0.5f, 0f);
        okRT.pivot = new Vector2(0.5f, 0f);
        okRT.sizeDelta = new Vector2(380f, 130f);
        okRT.anchoredPosition = new Vector2(120f, 60f);
        ok.GetComponent<Image>().color = new Color(0.20f, 0.62f, 0.32f, 1f);
        ok.GetComponent<Button>().onClick.AddListener(OnConfirmClicked);
        TextMeshProUGUI okLbl = CreateText(ok.transform, "Label", "CONFIRM", 46f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform okLblRT = (RectTransform)okLbl.transform;
        okLblRT.anchorMin = Vector2.zero; okLblRT.anchorMax = Vector2.one;
        okLblRT.offsetMin = Vector2.zero; okLblRT.offsetMax = Vector2.zero;
        okLbl.color = Color.white;

        // Cancel button
        GameObject cancel = new GameObject("Cancel", typeof(RectTransform), typeof(Image), typeof(Button));
        cancel.transform.SetParent(panel.transform, false);
        RectTransform cRT = (RectTransform)cancel.transform;
        cRT.anchorMin = new Vector2(0.5f, 0f); cRT.anchorMax = new Vector2(0.5f, 0f);
        cRT.pivot = new Vector2(0.5f, 0f);
        cRT.sizeDelta = new Vector2(280f, 130f);
        cRT.anchoredPosition = new Vector2(-220f, 60f);
        cancel.GetComponent<Image>().color = new Color(0.55f, 0.20f, 0.20f, 1f);
        cancel.GetComponent<Button>().onClick.AddListener(OnCancelClicked);
        TextMeshProUGUI cLbl = CreateText(cancel.transform, "Label", "CANCEL", 46f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform cLblRT = (RectTransform)cLbl.transform;
        cLblRT.anchorMin = Vector2.zero; cLblRT.anchorMax = Vector2.one;
        cLblRT.offsetMin = Vector2.zero; cLblRT.offsetMax = Vector2.zero;
        cLbl.color = Color.white;

        canvasGo.SetActive(false);
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string content, float size, FontStyles style, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = Color.white;
        t.raycastTarget = false;
        return t;
    }
}
