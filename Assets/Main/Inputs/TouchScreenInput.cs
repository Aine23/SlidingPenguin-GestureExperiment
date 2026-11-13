using UnityEngine;

/// <summary>
/// タッチスクリーン条件で使用する入力クラス。
/// 
/// 【操作方法】
///   - 画面に触れながらスワイプ → 方向入力
///   - タッチ開始点からの移動方向・距離で方向を決定する
///   - 指を離すと方向はゼロに戻る
/// 
/// Unityエディタ上でのデバッグ時はマウス入力をタッチとして代替する。
/// </summary>
public class TouchScreenInput : IInput
{
    public Vector3 Direction { get; set; }
    public bool Submit { get; set; }
    public bool Pause { get; set; }

    private Vector2 touchStartPos;
    private bool isTouching = false;

    // この距離（px）以上スワイプした場合に方向を確定する
    private const float DeadZone = 15f;

    public void UpdateInput()
    {
        if (PauseUtility.IsPause)
        {
            Direction = Vector3.zero;
            Submit = false;
            Pause = Input.GetButtonDown("Cancel");
            return;
        }

        // --- タッチ入力（実機） ---
        if (Input.touchCount > 0)
        {
            UpdateFromTouch(Input.GetTouch(0));
        }
#if UNITY_EDITOR
        // --- エディタ上でのデバッグ用：マウスでタッチを代替 ---
        else
        {
            UpdateFromMouseFallback();
        }
#endif

        Submit = false;
        Pause = Input.GetButtonDown("Cancel");
    }

    private void UpdateFromTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartPos = touch.position;
                isTouching = true;
                Direction = Vector3.zero;
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isTouching)
                {
                    Vector2 delta = touch.position - touchStartPos;
                    Direction = delta.magnitude >= DeadZone
                        ? new Vector3(delta.normalized.x, 0f, delta.normalized.y)
                        : Vector3.zero;
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isTouching = false;
                Direction = Vector3.zero;
                break;
        }
    }

#if UNITY_EDITOR
    private Vector2 editorTouchStart;
    private bool editorTouching = false;

    private void UpdateFromMouseFallback()
    {
        if (Input.GetMouseButtonDown(0))
        {
            editorTouchStart = Input.mousePosition;
            editorTouching = true;
            Direction = Vector3.zero;
        }
        if (editorTouching && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - editorTouchStart;
            Direction = delta.magnitude >= DeadZone
                ? new Vector3(delta.normalized.x, 0f, delta.normalized.y)
                : Vector3.zero;
        }
        if (Input.GetMouseButtonUp(0))
        {
            editorTouching = false;
            Direction = Vector3.zero;
        }
    }
#endif
}
