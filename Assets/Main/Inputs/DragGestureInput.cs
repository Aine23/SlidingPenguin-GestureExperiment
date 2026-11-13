using UnityEngine;

/// <summary>
/// マウス条件・タッチパッド条件で使用する入力クラス。
/// 
/// 【操作方法】
///   - 左クリック（またはタッチパッド押し込み）を保持しながらドラッグ → 方向入力
///   - ドラッグ距離がdeadZoneを超えた時点で方向が確定する
///   - クリックを離すと方向はゼロに戻る
/// 
/// マウスとタッチパッドはUnity上で同じMouseButton(0)として扱われるため、
/// 実験者が物理的にデバイスを切り替えることで条件を分ける。
/// </summary>
public class DragGestureInput : IInput
{
    public Vector3 Direction { get; set; }
    public bool Submit { get; set; }
    public bool Pause { get; set; }

    // ドラッグ開始位置
    private Vector2 dragStartPos;
    private bool isDragging = false;

    // この距離（px）以上ドラッグした場合に方向を確定する
    private const float DeadZone = 10f;

    public void UpdateInput()
    {
        // ポーズ中は入力をゼロに
        if (PauseUtility.IsPause)
        {
            Direction = Vector3.zero;
            Submit = false;
            Pause = Input.GetButtonDown("Cancel");
            return;
        }

        // --- 方向入力 ---
        if (Input.GetMouseButtonDown(0))
        {
            // クリック開始：ドラッグ起点を記録
            dragStartPos = Input.mousePosition;
            isDragging = true;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentPos = Input.mousePosition;
            Vector2 delta = currentPos - dragStartPos;

            if (delta.magnitude >= DeadZone)
            {
                // ドラッグ方向を正規化してXZ平面の方向に変換
                Vector2 normalized = delta.normalized;
                Direction = new Vector3(normalized.x, 0f, normalized.y);
            }
            else
            {
                // DeadZone未満はニュートラル
                Direction = Vector3.zero;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            // クリック解放：方向をリセット
            isDragging = false;
            Direction = Vector3.zero;
        }

        // --- スピードアップ（Submit）---
        // ダブルクリック相当の操作をSubmitに割り当てる場合はここに追加
        Submit = false;

        // --- ポーズ ---
        Pause = Input.GetButtonDown("Cancel");
    }
}
