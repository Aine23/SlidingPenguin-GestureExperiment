using UnityEngine;

/// <summary>
/// 実験用入力をOVGL既存の InputDataManager に橋渡しするコンポーネント。
/// 
/// 【既存コードへの変更箇所】
///   このスクリプトをシーン内の適当なGameObjectにアタッチするだけでOK。
///   InputDataManager が Update() で DefaultInput を呼ぶ前に、
///   このスクリプトが条件に応じた入力クラスへ差し替える。
/// 
/// 【具体的な変更が必要な箇所（1か所のみ）】
///   InputDataManager.cs の初期化部分（Awake or Start）で
///   input フィールドに DefaultInput をセットしている箇所を、
///   以下のように変更する：
/// 
///     // 変更前
///     input = new DefaultInput();
/// 
///     // 変更後
///     input = ExperimentManager.Instance != null && ExperimentManager.Instance.IsInitialized
///             ? ExperimentManager.Instance.CreateInput()
///             : new DefaultInput();  // フォールバック（通常プレイ用）
/// 
///   これだけで、実験条件に応じた入力クラスが自動的に使用される。
/// </summary>
public class ExperimentInputBridge : MonoBehaviour
{
    private void Start()
    {
        // ExperimentManagerが未初期化の場合（通常プレイ）は何もしない
        if (ExperimentManager.Instance == null || !ExperimentManager.Instance.IsInitialized)
        {
            Debug.Log("[ExperimentInputBridge] 実験モードではないため、通常入力を使用します。");
            return;
        }

        // ExperimentDataLoggerが存在しない場合は生成
        if (ExperimentDataLogger.Instance == null)
        {
            var go = new GameObject("ExperimentDataLogger");
            go.AddComponent<ExperimentDataLogger>();
        }

        // 記録開始
        ExperimentDataLogger.Instance.StartSession();
        Debug.Log($"[ExperimentInputBridge] 実験モード開始: {ExperimentManager.Instance.Condition}");
    }
}
