// 配置: Assets/_MRCharBase/Scripts/Services/External/
// 責務: ElevenLabs API を使った TTS（テキスト→音声）実装（§10.9）

using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// ElevenLabs リクエストボディ用ラッパークラス
// レスポンスは MP3 バイナリのため JSON 解析不要（04-platform-gotchas §2）
[Serializable]
internal class ElevenLabsRequest
{
    public string text;
    public string model_id;
}

/// <summary>
/// ITextToSpeechService の外部 API 実装。
/// ElevenLabs API に text を送信し、MP3 → AudioClip に変換して返す。
/// DownloadHandlerAudioClip を明示的に設定する（UnityWebRequestMultimedia.GetAudioClip() 使用禁止）。
/// （§6・04-platform-gotchas §2 準拠）
/// </summary>
public class ExternalTextToSpeechClient : ITextToSpeechService
{
    private const string BaseUrl = "https://api.elevenlabs.io/v1/text-to-speech";
    private const string ModelId = "eleven_multilingual_v2";

    private readonly string _apiKey;
    private readonly string _voiceId;

    public ExternalTextToSpeechClient(AppConfig config)
    {
        _apiKey  = config.elevenLabsApiKey;
        _voiceId = config.elevenLabsVoiceId;
    }

    public async UniTask<AudioClip> SynthesizeAsync(string text)
    {
        string url = $"{BaseUrl}/{_voiceId}";

        var requestBody = new ElevenLabsRequest
        {
            text     = text,
            model_id = ModelId
        };

        byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestBody));

        using var www = new UnityWebRequest(url, "POST");
        www.uploadHandler   = new UploadHandlerRaw(bodyBytes);
        // MP3 受信のため DownloadHandlerAudioClip を明示的に設定（省略禁止）
        // UnityWebRequestMultimedia.GetAudioClip() では MP3 streaming に失敗するケースがある
        www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("xi-api-key",   _apiKey);

        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            throw new Exception($"[TTS] API通信失敗: {www.error}");

        return DownloadHandlerAudioClip.GetContent(www);
    }
}
