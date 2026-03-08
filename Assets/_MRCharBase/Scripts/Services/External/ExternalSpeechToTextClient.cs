// 配置: Assets/_MRCharBase/Scripts/Services/External/
// 責務: OpenAI Whisper API を使った STT（音声→テキスト）実装（§10.9）

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Whisper API レスポンス {"text": "..."} のデシリアライズ用ラッパークラス。
/// JsonUtility の制限により [Serializable] ラッパーが必要（04-platform-gotchas §3）。
/// </summary>
[Serializable]
internal class WhisperResponse
{
    public string text;
}

/// <summary>
/// ISpeechToTextService の外部 API 実装。
/// OpenAI Whisper API（https://api.openai.com/v1/audio/transcriptions）に
/// multipart/form-data 形式で WAV データを送信し、テキストを取得する。
/// </summary>
public class ExternalSpeechToTextClient : ISpeechToTextService
{
    private const string Endpoint = "https://api.openai.com/v1/audio/transcriptions";
    private const string Model    = "whisper-1";

    private readonly string _apiKey;

    public ExternalSpeechToTextClient(AppConfig config)
    {
        _apiKey = config.openAIApiKey;
    }

    public async UniTask<string> TranscribeAsync(byte[] wavData)
    {
        // multipart/form-data フォームを構築
        var form = new WWWForm();
        form.AddField("model", Model);
        form.AddBinaryData("file", wavData, "audio.wav", "audio/wav");

        using var www = UnityWebRequest.Post(Endpoint, form);
        www.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            throw new Exception($"[STT] API通信失敗: {www.error} / {www.downloadHandler.text}");

        var response = JsonUtility.FromJson<WhisperResponse>(www.downloadHandler.text);
        return response.text;
    }
}
