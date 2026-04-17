// ExternalOpenAiTtsClient.cs
// 責務: OpenAI TTS API (POST /v1/audio/speech) を呼び出し AudioClip を返す
// 配置: Assets/_MRCharBase/Scripts/Services/External/ExternalOpenAiTtsClient.cs
// エンジン選択: AppSetup.TtsEngine.OpenAI 選択時に AppSetup から生成・注入される
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ExternalOpenAiTtsClient : ITextToSpeechService
{
    private const string Endpoint     = "https://api.openai.com/v1/audio/speech";
    private const string FallbackModel = "tts-1";
    private const string FallbackVoice = "alloy";

    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _voice;

    public ExternalOpenAiTtsClient(AppConfig config)
    {
        _apiKey = config.openAIApiKey;
        _model  = string.IsNullOrEmpty(config.openAiTtsModel) ? FallbackModel : config.openAiTtsModel;
        _voice  = string.IsNullOrEmpty(config.openAiTtsVoice) ? FallbackVoice : config.openAiTtsVoice;
    }

    public async UniTask<AudioClip> SynthesizeAsync(string text)
    {
        // リクエストボディを JsonUtility でシリアライズ
        var requestBody = new OpenAiTtsRequest
        {
            model           = _model,
            input           = text,
            voice           = _voice,
            response_format = "mp3"
        };
        string json      = JsonUtility.ToJson(requestBody);
        byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(json);

        // MP3 受信のため DownloadHandlerAudioClip を明示設定（04_platform_gotchas §2 準拠）
        // UnityWebRequestMultimedia.GetAudioClip() は MP3 streaming に失敗するケースがあるため使用禁止
        using var www = new UnityWebRequest(Endpoint, "POST");
        www.uploadHandler   = new UploadHandlerRaw(bodyBytes);
        www.downloadHandler = new DownloadHandlerAudioClip(Endpoint, AudioType.MPEG);

        www.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
        www.SetRequestHeader("Content-Type",  "application/json");

        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            throw new Exception($"[OpenAI TTS] 通信失敗: {www.error} / {www.downloadHandler.text}");

        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        if (clip == null)
            throw new Exception("[OpenAI TTS] AudioClip の取得に失敗しました。");

        return clip;
    }
}

// ── JsonUtility シリアライズ用ラッパー（クラス外・トップレベル定義）──────────────
[Serializable]
internal class OpenAiTtsRequest
{
    public string model;
    public string input;
    public string voice;
    public string response_format;
}