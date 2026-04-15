// 配置: Assets/_MRCharBase/Scripts/Services/Interfaces/
// 責務: TTS（テキスト→音声）サービスの抽象化（§10.1）

using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// TTS（音声合成）サービスのインターフェース。
/// 実装: ExternalElevenLabsClient（本番）/ MockTextToSpeechService（Mock）
/// </summary>
public interface ITextToSpeechService
{
    UniTask<AudioClip> SynthesizeAsync(string text);
}
