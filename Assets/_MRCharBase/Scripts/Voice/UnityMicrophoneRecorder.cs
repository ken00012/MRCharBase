// 配置: Assets/_MRCharBase/Scripts/Voice/
// 責務: Unity の Microphone API を使って録音し、WAV byte[] を返す（§10.6）

using System;
using UnityEngine;

/// <summary>
/// IAudioRecorder の Unity Microphone API 実装。
/// VoiceInputHandler の内部実装として使用される。
/// 将来: MetaVoiceRecorder への内部差し替えが可能（§10.6）。
/// </summary>
public class UnityMicrophoneRecorder : IAudioRecorder
{
    private AudioClip _clip;
    private bool _isRecording = false;  // Microphone.Start() が失敗した場合の保護フラグ

    private const int MaxSeconds = 30;    // 最大録音秒数（§10.6・04-platform-gotchas §4）
    private const int SampleRate  = 44100; // Unity Microphone のデフォルト（§10.6）

    public void StartRecording()
    {
        _clip = Microphone.Start(null, false, MaxSeconds, SampleRate);
        _isRecording = (_clip != null); // null のとき false のまま（Quest実機マイク保護）
    }

    public byte[] StopRecording()
    {
        // ① 未録音ガード（省略禁止: Quest実機でマイク取得失敗時の保護）
        if (!_isRecording) return Array.Empty<byte>();
        // ② フラグをリセット
        _isRecording = false;

        // ③ 録音位置を取得
        int pos = Microphone.GetPosition(null);
        // ④ 録音停止
        Microphone.End(null);

        // ⑤ GetPosition == 0 ガード（Quest実機バグ対策: §04-platform-gotchas §4）
        if (pos <= 0 || _clip == null) return Array.Empty<byte>();

        // ⑥ PCM データを取り出す（チャンネル数は _clip.channels で動的取得・ハードコード禁止）
        float[] samples = new float[pos * _clip.channels];
        _clip.GetData(samples, 0);

        // ⑦ WAV にエンコードして返す
        return WavUtility.FromAudioClipData(samples, pos, _clip.channels, SampleRate);
    }
}
