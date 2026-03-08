// 配置: Assets/_MRCharBase/Scripts/Core/
// 責務: キャラクターの状態を定義する enum（§10.3）

/// <summary>
/// キャラクターの状態を表す列挙型。
/// CharacterStateController が唯一の管理者であり、
/// SetState() メソッド経由でのみ変更される。
/// </summary>
public enum CharacterState
{
    Idle,       // 待機中
    Listening,  // 録音中
    Thinking,   // AI処理中
    Speaking    // 話し中
}
