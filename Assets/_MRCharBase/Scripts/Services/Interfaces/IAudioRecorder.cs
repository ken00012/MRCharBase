// 配置: Assets/_MRCharBase/Scripts/Services/Interfaces/
// 責務: 録音機能の抽象化（§10.1）

/// <summary>
/// 録音サービスのインターフェース。
/// 実装: UnityMicrophoneRecorder（VoiceInputHandler内部で使用）
/// 将来: MetaVoiceRecorder への差し替えが可能（§10.1）。
/// useMock に関わらず常にMonoBehaviour実装（VoiceInputHandler）を使用する。
/// </summary>
public interface IAudioRecorder
{
    void StartRecording();
    byte[] StopRecording(); // WAV byte[] を返す
}
