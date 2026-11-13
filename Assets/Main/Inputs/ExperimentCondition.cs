/// <summary>
/// 実験の入力条件を表す列挙型
/// </summary>
public enum InputCondition
{
    Mouse,        // マウス：左クリック保持 + ドラッグで方向入力
    TouchScreen,  // タッチスクリーン：スワイプで方向入力
    Touchpad,     // タッチパッド：押し込み（左クリック保持）+ スワイプで方向入力
                  // ※ MouseとTouchpadはソフトウェア的には同一処理。
                  //   実験者が物理的にデバイスを切り替えること。
}
