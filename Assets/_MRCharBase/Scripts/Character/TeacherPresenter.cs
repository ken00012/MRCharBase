// 配置: キャラクター Root GameObject（CharacterStateController と同じ）
// 責務: 状態に応じたキャラクター表示制御（§10.5）

using UnityEngine;

/// <summary>
/// キャラクターの状態別表示を担当する MonoBehaviour。
/// CharacterStateController.SetState() 経由で OnStateChanged() が呼ばれる。
/// 将来: 各状態メソッドに Animator.SetTrigger() / OVRLipSync を追加するだけでよい（§10.5）。
/// </summary>
public class TeacherPresenter : MonoBehaviour
{
    // 将来: [SerializeField] private Animator _animator;
    // 将来: [SerializeField] private OVRLipSyncContext _lipSync;

    /// <summary>CharacterStateController から状態変更通知を受け取る。</summary>
    public void OnStateChanged(CharacterState state)
    {
        switch (state)
        {
            case CharacterState.Idle:      Idle();      break;
            case CharacterState.Listening: Listening(); break;
            case CharacterState.Thinking:  Thinking();  break;
            case CharacterState.Speaking:  Speaking();  break;
        }
    }

    private void Idle()      => Debug.Log("[Teacher] Idle");
    private void Listening() => Debug.Log("[Teacher] Listening");
    private void Thinking()  => Debug.Log("[Teacher] Thinking");
    private void Speaking()  => Debug.Log("[Teacher] Speaking");
    // 将来: 各メソッドに _animator.SetTrigger("xxx") を追加
}
