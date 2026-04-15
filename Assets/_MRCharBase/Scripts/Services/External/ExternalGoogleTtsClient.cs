// 配置: Assets/_MRCharBase/Scripts/Services/External/
// 責務: Google Cloud Text-to-Speech API を使った TTS（テキスト→音声）実装（§10.9）

using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ITextToSpeechService の Google Cloud TTS API 実装。
/// JSON レスポンスの audioContent（Base64 MP3）を一時ファイル経由で AudioClip に変換して返す。
/// DownloadHandlerAudioClip を明示的に設定する（UnityWebRequestMultimedia.GetAudioClip() 使用禁止）。
/// （§6・04-platform-gotchas §2 準拠）
/// </summary>
public class ExternalGoogleTtsClient : ITextToSpeechService
{
    private const string BaseUrl          = "https://texttospeech.googleapis.com/v1/text:synthesize";
    private const string DefaultLangCode  = "ja-JP";
    private const string DefaultVoiceName = "ja-JP-Neural2-B";

    private readonly string _apiKey;
    private readonly string _languageCode;
    private readonly string _voiceName;

    public ExternalGoogleTtsClient(AppConfig config)
    {
        _apiKey       = config.googleTtsApiKey;
        // 空文字の場合はデフォルト値にフォールバック
        _languageCode = string.IsNullOrEmpty(config.googleTtsLanguageCode) ? DefaultLangCode  : config.googleTtsLanguageCode;
        _voiceName    = string.IsNullOrEmpty(config.googleTtsVoiceName)    ? DefaultVoiceName : config.googleTtsVoiceName;
    }

    public async UniTask<AudioClip> SynthesizeAsync(string text)
    {
        string url = $"{BaseUrl}?key={_apiKey}";

        // リクエストボディ構築
        var requestBody = new GoogleTtsSynthesizeRequest
        {
            input = new GoogleTtsInput { text = text },
            voice = new GoogleTtsVoice
            {
                languageCode = _languageCode,
                name         = _voiceName
            },
            audioConfig = new GoogleTtsAudioConfig { audioEncoding = "MP3" }
        };

        byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestBody));

        // ① Google TTS API への POST リクエスト
        string audioContent;
        using (var www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler   = new UploadHandlerRaw(bodyBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                throw new Exception($"[GoogleTTS] API通信失敗: {www.error}");

            // JsonUtility でレスポンスをパース
            var response = JsonUtility.FromJson<GoogleTtsSynthesizeResponse>(www.downloadHandler.text);
            if (response == null || string.IsNullOrEmpty(response.audioContent))
                throw new Exception("[GoogleTTS] レスポンスに audioContent が含まれていません。");

            audioContent = response.audioContent;
        }

        // ② Base64 → byte[] デコード
        byte[] mp3Bytes;
        try
        {
            mp3Bytes = Convert.FromBase64String(audioContent);
        }
        catch (Exception e)
        {
            throw new Exception($"[GoogleTTS] Base64 デコード失敗: {e.Message}");
        }

        // ③ 一時ファイルへ書き出し（Android の temporaryCachePath を使用：04-platform-gotchas 準拠）
        string tempPath = Path.Combine(Application.temporaryCachePath, "temp_tts.mp3");
        File.WriteAllBytes(tempPath, mp3Bytes);

        // ④ file:// URI 経由で AudioClip を取得
        string fileUri = "file://" + tempPath;
        AudioClip clip;

        // using ブロック内で GetContent() を呼び、スコープ外の変数に格納してからブロックを抜ける（仕様必須）
        using (var fileReq = new UnityWebRequest(fileUri, "GET"))
        {
            fileReq.downloadHandler = new DownloadHandlerAudioClip(fileUri, AudioType.MPEG);

            await fileReq.SendWebRequest();

            if (fileReq.result != UnityWebRequest.Result.Success)
                throw new Exception($"[GoogleTTS] 一時ファイル読み込み失敗: {fileReq.error}");

            // using ブロック内で取得し、スコープ外変数に格納
            clip = DownloadHandlerAudioClip.GetContent(fileReq);
        }
        // ここでは GetContent() を呼ばない（using ブロック外）

        if (clip == null)
            throw new Exception("[GoogleTTS] AudioClip の取得に失敗しました（clip が null）。");

        // ⑤ 一時ファイルを削除（失敗は警告のみ。AudioClip は返却する）
        try
        {
            File.Delete(tempPath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GoogleTTS] 一時ファイルの削除に失敗しました（無視）: {e.Message}");
        }

        return clip;
    }
}

// JsonUtility 対応のため、private ネストクラスは使用禁止。
// トップレベル（クラス外）に定義することで JsonUtility.ToJson/FromJson() が正常動作する（04-platform-gotchas 準拠）。
[Serializable]
internal class GoogleTtsSynthesizeRequest
{
    public GoogleTtsInput       input;
    public GoogleTtsVoice       voice;
    public GoogleTtsAudioConfig audioConfig;
}

[Serializable]
internal class GoogleTtsInput
{
    public string text;
}

[Serializable]
internal class GoogleTtsVoice
{
    public string languageCode;
    public string name;
}

[Serializable]
internal class GoogleTtsAudioConfig
{
    public string audioEncoding; // "MP3" 固定
}

[Serializable]
internal class GoogleTtsSynthesizeResponse
{
    public string audioContent; // Base64 エンコードされた MP3 データ
}
