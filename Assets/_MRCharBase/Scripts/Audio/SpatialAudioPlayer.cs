// 配置: キャラクターの Head / Mouth GameObject（Root 不可。音源位置がずれる）
// 責務: キャラクターの位置から空間音響として再生する（§10.7）

using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ISpatialAudioPlayer の MonoBehaviour 実装。
/// AudioSource.clip に代入して Play() する方式で再生完了を isPlaying で正確に検知する。
/// PlayOneShot() は isPlaying が正確に検知できないため使用禁止（§10.7・04-platform-gotchas §5）。
/// </summary>
public class SpatialAudioPlayer : MonoBehaviour, ISpatialAudioPlayer
{
    [SerializeField] private AudioSource audioSource;

    public async UniTask PlayAsync(AudioClip clip)
    {
        // Inspector 未設定または clip 引数エラーの場合は即 return。
        // 音は出ないが Controller 側には制御が戻り、Idle 状態へ復帰するためアプリは進行可能。
        if (audioSource == null || clip == null) return; // null ガード（省略禁止）

        audioSource.spatialBlend = 1.0f;  // 3D空間音響（省略すると 2D 再生になる）
        audioSource.clip = clip;          // PlayOneShot ではなく clip に代入
        audioSource.Play();

        // clip に代入した場合は isPlaying で正確に完了検知できる。
        // GetCancellationTokenOnDestroy: GameObject 廃棄時（シーン遷移・クラッシュ）に自動キャンセル。
        await UniTask.WaitWhile(() => audioSource.isPlaying,
            cancellationToken: this.GetCancellationTokenOnDestroy());

        // 再生完了後にメモリを解放（設計書追記 2026-03-08）
        audioSource.clip = null;
        Object.Destroy(clip);
    }
}
