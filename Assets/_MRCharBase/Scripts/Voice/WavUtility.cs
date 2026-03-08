// 配置: Assets/_MRCharBase/Scripts/Voice/
// 責務: PCM float[] データを WAV byte[] にエンコードする（§10.6）

using System;

/// <summary>
/// Unity の Microphone API が返す PCM float[] データを
/// Whisper API に送信できる WAV byte[] に変換するユーティリティ。
/// フォーマット: PCM 16bit / 44100Hz / モノラルまたはステレオ（channels で動的対応）
/// </summary>
public static class WavUtility
{
    private const int HeaderSize    = 44; // WAV ヘッダーの固定サイズ（バイト）
    private const int BitsPerSample = 16; // PCM 16bit 固定

    /// <summary>
    /// PCM float[] データを WAV byte[] に変換する。
    /// </summary>
    /// <param name="samples">
    ///   AudioClip.GetData() で取得した PCM float[] データ。
    ///   サイズは sampleCount * channels であること。
    /// </param>
    /// <param name="sampleCount">録音サンプル数（Microphone.GetPosition() の戻り値）</param>
    /// <param name="channels">チャンネル数（_clip.channels で動的取得。ハードコード禁止）</param>
    /// <param name="sampleRate">サンプルレート（44100 固定）</param>
    /// <returns>WAV フォーマットの byte[] データ</returns>
    public static byte[] FromAudioClipData(float[] samples, int sampleCount, int channels, int sampleRate)
    {
        int dataSize   = sampleCount * channels * (BitsPerSample / 8); // PCM データサイズ（バイト）
        int fileSize   = HeaderSize + dataSize;
        byte[] wav     = new byte[fileSize];

        // --- RIFF チャンク ---
        Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, wav, 0,  4);
        Buffer.BlockCopy(BitConverter.GetBytes(fileSize - 8),          0, wav, 4,  4); // ChunkSize
        Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, wav, 8,  4);

        // --- fmt サブチャンク ---
        Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, wav, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(16),                    0, wav, 16, 4); // Subchunk1Size (PCM=16)
        Buffer.BlockCopy(BitConverter.GetBytes((short)1),              0, wav, 20, 2); // AudioFormat (PCM=1)
        Buffer.BlockCopy(BitConverter.GetBytes((short)channels),       0, wav, 22, 2); // NumChannels
        Buffer.BlockCopy(BitConverter.GetBytes(sampleRate),            0, wav, 24, 4); // SampleRate
        Buffer.BlockCopy(BitConverter.GetBytes(sampleRate * channels * (BitsPerSample / 8)), 0, wav, 28, 4); // ByteRate
        Buffer.BlockCopy(BitConverter.GetBytes((short)(channels * (BitsPerSample / 8))),     0, wav, 32, 2); // BlockAlign
        Buffer.BlockCopy(BitConverter.GetBytes((short)BitsPerSample),  0, wav, 34, 2); // BitsPerSample

        // --- data サブチャンク ---
        Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("data"), 0, wav, 36, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(dataSize),              0, wav, 40, 4); // Subchunk2Size

        // --- PCM データ（float → 16bit short に変換）---
        int offset = HeaderSize;
        int total  = sampleCount * channels;
        for (int i = 0; i < total; i++)
        {
            // float（-1.0〜1.0）を short（-32768〜32767）に変換
            short pcm = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, samples[i] * 32767f));
            wav[offset]     = (byte)(pcm & 0xFF);
            wav[offset + 1] = (byte)((pcm >> 8) & 0xFF);
            offset += 2;
        }

        return wav;
    }
}
