// 配置: Assets/_MRCharBase/Scripts/Services/Mock/
// 責務: STT Mock 実装（ネットワーク不要で全体フロー確認用）（§10.9・02-architecture §7）

using Cysharp.Threading.Tasks;

/// <summary>
/// ISpeechToTextService の Mock 実装。
/// 遅延を含めることで状態遷移テストが有効になる（遅延なし不可・02-architecture §7）。
/// useMock = true 時に AppSetup から注入される。
/// </summary>
public class MockSpeechToTextService : ISpeechToTextService
{
    public async UniTask<string> TranscribeAsync(byte[] wavData)
    {
        await UniTask.Delay(1000); // 遅延必須（省略禁止）
        return "これはMockの質問テキストです。";
    }
}
