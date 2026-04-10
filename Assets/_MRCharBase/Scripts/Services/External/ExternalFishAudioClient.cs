// 配置: Assets/_MRCharBase/Scripts/Services/External/
// 責務: Fish Audio API を使った TTS（テキスト→音声）実装（§10.9）

using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ITextToSpeechService の Fish Audio API 実装。
/// Fish Audio API に text を送信し、MP3 → AudioClip に変換して返す。
/// DownloadHandlerAudioClip を明示的に設定する（UnityWebRequestMultimedia.GetAudioClip() 使用禁止）。
/// モデルはリクエストヘッダーで指定する Fish Audio 仕様に準拠（§6・04-platform-gotchas §2 準拠）。
/// </summary>
public class ExternalFishAudioClient : ITextToSpeechService
{
    private const string BaseUrl      = "https://api.fish.audio/v1/tts";
    private const string DefaultModel = "s2-pro"; // fishAudioModel が空文字の場合のフォールバック値

    private readonly string _apiKey;
    private readonly string _referenceId;
    private readonly string _model;

    public ExternalFishAudioClient(AppConfig config)
    {
        _apiKey      = config.fishAudioApiKey;
        _referenceId = config.fishAudioReferenceId;
        // 空文字は DefaultModel にフォールバック
        _model = string.IsNullOrEmpty(config.fishAudioModel) ? DefaultModel : config.fishAudioModel;
    }

    public async UniTask<AudioClip> SynthesizeAsync(string text)
    {
        var requestBody = new FishAudioTtsRequest
        {
            text         = text,
            reference_id = _referenceId,
            format       = "mp3",
            latency      = "normal"
        };

        byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestBody));

        using var www = new UnityWebRequest(BaseUrl, "POST");
        www.uploadHandler   = new UploadHandlerRaw(bodyBytes);
        // MP3 受信のため DownloadHandlerAudioClip を明示的に設定（UnityWebRequestMultimedia.GetAudioClip() 禁止）
        www.downloadHandler = new DownloadHandlerAudioClip(BaseUrl, AudioType.MPEG);
        www.SetRequestHeader("Authorization",  $"Bearer {_apiKey}");
        www.SetRequestHeader("Content-Type",   "application/json");
        www.SetRequestHeader("model",          _model); // Fish Audio はモデルをヘッダーで指定する仕様

        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            throw new Exception($"[FishAudio TTS] API通信失敗: {www.error}");

        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        if (clip == null)
            throw new Exception("[FishAudio TTS] AudioClip の取得に失敗しました（clip が null）");

        return clip;
    }
}

// JsonUtility 対応のため、private ネストクラスは使用禁止。
// トップレベル（クラス外）に定義することで JsonUtility.ToJson() が正常動作する（04-platform-gotchas 準拠）。
[Serializable]
internal class FishAudioTtsRequest
{
    public string text;
    public string reference_id;
    public string format;   // "mp3" 固定
    public string latency;  // "normal" 固定
    // 将来拡張候補: temperature, top_p, prosody（speed/volume）等
}
