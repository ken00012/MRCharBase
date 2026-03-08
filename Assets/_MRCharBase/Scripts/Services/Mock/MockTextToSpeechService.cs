// 配置: Assets/_MRCharBase/Scripts/Services/Mock/
// 責務: TTS Mock 実装（ネットワーク不要で全体フロー確認用）（§10.9・02-architecture §7）

using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ITextToSpeechService の Mock 実装。
/// 遅延を含めることで状態遷移テストが有効になる（遅延なし不可・02-architecture §7）。
/// AudioClip.Create だけではゴミデータが混入する場合があるため SetData でゼロ埋め必須。
/// useMock = true 時に AppSetup から注入される。
/// </summary>
public class MockTextToSpeechService : ITextToSpeechService
{
    public async UniTask<AudioClip> SynthesizeAsync(string text)
    {
        await UniTask.Delay(500); // 遅延必須（省略禁止）
        var clip = AudioClip.Create("mock", 44100, 1, 44100, false);
        clip.SetData(new float[44100], 0); // ゼロ埋め必須（Create のみではゴミデータが混入する場合がある）
        return clip;
    }
}
