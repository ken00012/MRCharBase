// 配置: Assets/_MRCharBase/Scripts/Services/Interfaces/
// 責務: STT（音声→テキスト）サービスの抽象化（§10.1）

using Cysharp.Threading.Tasks;

/// <summary>
/// STT（音声認識）サービスのインターフェース。
/// 実装: ExternalSpeechToTextClient（本番）/ MockSpeechToTextService（Mock）
/// </summary>
public interface ISpeechToTextService
{
    UniTask<string> TranscribeAsync(byte[] wavData);
}
