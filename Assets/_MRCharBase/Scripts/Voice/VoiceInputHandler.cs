// 配置: XR Rig（Player）配下の GameObject
// 責務: IAudioRecorder を実装し、マイク録音のみ担当する（§10.6）

using UnityEngine;

/// <summary>
/// IAudioRecorder の MonoBehaviour 実装。
/// 内部実装を UnityMicrophoneRecorder に委譲することで、
/// 将来 MetaVoiceRecorder への内部差し替えが可能（§10.6）。
/// AppSetup から IAudioRecorder として Inject される。
/// </summary>
public class VoiceInputHandler : MonoBehaviour, IAudioRecorder
{
    // IAudioRecorder 内部実装は UnityMicrophoneRecorder で抽象化
    // 将来: new MetaVoiceRecorder() に内部差し替え可能
    private IAudioRecorder _impl = new UnityMicrophoneRecorder();

    // IAudioRecorder 実装（_impl に委譲）
    public void StartRecording() => _impl.StartRecording();
    public byte[] StopRecording() => _impl.StopRecording();
}
