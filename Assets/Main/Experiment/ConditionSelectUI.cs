using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 実験開始前の条件選択画面を制御するUI。TextMeshPro対応版。
///
/// 【Unityエディタ上での設定手順】
///   1. Canvas以下の空のGameObject（ExperimentUI）にアタッチする
///   2. Inspectorから以下をアサインする：
///      - sessionIdInput      : TMP_InputField（参加者ID入力）
///      - mouseButton         : Button（マウス条件選択）
///      - touchpadButton      : Button（タッチパッド条件選択）
///      - touchscreenButton   : Button（タッチスクリーン条件選択）
///      - errorText           : TMP_Text（エラーメッセージ表示用）
///      - gameSceneName       : 遷移先ゲームシーンの名前
/// </summary>
public class ConditionSelectUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField sessionIdInput;
    [SerializeField] private Button mouseButton;
    [SerializeField] private Button touchpadButton;
    [SerializeField] private Button touchscreenButton;
    [SerializeField] private TMP_Text errorText;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    // 選択中の条件（ボタン押下で更新）
    private InputCondition? selectedCondition = null;

    private void Start()
    {
        // ExperimentManagerが存在しない場合は生成
        if (ExperimentManager.Instance == null)
        {
            var go = new GameObject("ExperimentManager");
            go.AddComponent<ExperimentManager>();
        }

        // ボタンのコールバック設定
        mouseButton.onClick.AddListener(() => SelectCondition(InputCondition.Mouse));
        touchpadButton.onClick.AddListener(() => SelectCondition(InputCondition.Touchpad));
        touchscreenButton.onClick.AddListener(() => SelectCondition(InputCondition.TouchScreen));

        errorText.text = string.Empty;
        UpdateButtonVisuals();
    }

    private void SelectCondition(InputCondition condition)
    {
        selectedCondition = condition;
        errorText.text = string.Empty;
        UpdateButtonVisuals();
        Debug.Log($"[ConditionSelectUI] 条件選択: {condition}");
    }

    /// <summary>「実験開始」ボタンから呼ぶ（OnClick）</summary>
    public void OnStartButtonClicked()
    {
        string sessionId = sessionIdInput.text.Trim();
        if (string.IsNullOrEmpty(sessionId))
        {
            errorText.text = "Please enter participant ID.";
            return;
        }
        if (selectedCondition == null)
        {
            errorText.text = "Please select a condition.";
            return;
        }

        ExperimentManager.Instance.Initialize(sessionId, selectedCondition.Value);
        SceneManager.LoadScene(gameSceneName);
    }

    /// 選択中のボタンを視覚的に強調する
    private void UpdateButtonVisuals()
    {
        SetButtonHighlight(mouseButton,       selectedCondition == InputCondition.Mouse);
        SetButtonHighlight(touchpadButton,    selectedCondition == InputCondition.Touchpad);
        SetButtonHighlight(touchscreenButton, selectedCondition == InputCondition.TouchScreen);
    }

    private void SetButtonHighlight(Button button, bool isSelected)
    {
        // ボタン内のTMP_Textを取得してテキスト色で選択状態を表現する
        var tmp = button.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.color = isSelected ? new Color(0.1f, 0.6f, 0.1f) : Color.black;
        }

        // ボタン枠の色も変える（Image経由）
        var image = button.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            image.color = isSelected ? new Color(0.85f, 1f, 0.85f) : Color.white;
        }
    }
}