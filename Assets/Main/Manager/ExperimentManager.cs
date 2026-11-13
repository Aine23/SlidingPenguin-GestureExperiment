using UnityEngine;

/// <summary>
/// 実験全体を管理するシングルトン。
/// セッションID・入力条件を保持し、シーンをまたいで参照できる。
/// ConditionSelectUIから初期化され、以降は読み取り専用として使う。
/// </summary>
public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance { get; private set; }

    // ===== 実験情報 =====
    public string SessionId { get; private set; }
    public InputCondition Condition { get; private set; }

    // ===== 初期化済みフラグ =====
    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// ConditionSelectUIから呼ばれる初期化メソッド。
    /// </summary>
    public void Initialize(string sessionId, InputCondition condition)
    {
        SessionId = sessionId;
        Condition = condition;
        IsInitialized = true;

        Debug.Log($"[ExperimentManager] SessionId={SessionId}, Condition={Condition}");
    }

    /// <summary>
    /// 条件に応じたIInputインスタンスを生成して返す。
    /// InputDataManagerのセットアップ時に呼ぶ。
    /// </summary>
    public IInput CreateInput()
    {
        switch (Condition)
        {
            case InputCondition.Mouse:
            case InputCondition.Touchpad:
                // どちらも「クリック保持 + ドラッグ」。
                // 物理デバイスの違いはソフトウェアでは区別しない。
                return new DragGestureInput();
            case InputCondition.TouchScreen:
                return new TouchScreenInput();
            default:
                Debug.LogWarning("[ExperimentManager] 未知の条件です。Mouseにフォールバックします。");
                return new DragGestureInput();
        }
    }
}
