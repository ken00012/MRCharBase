// 配置: Assets/_MRCharBase/Scripts/Services/External/
// 責務: OpenAI Chat Completions API を使った LLM（回答生成）実装（§10.9）
// Stateless（1問1答）設計。会話履歴は将来拡張扱い（§10.9 明記）。

using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// ---------------------------------------------------------------
// リクエスト用ラッパークラス
// JsonUtility はルート配列非対応のため必ずクラスでラップする（04-platform-gotchas §3）
// ---------------------------------------------------------------

[Serializable]
internal class GptRequestMessage
{
    public string role;
    public string content;
}

[Serializable]
internal class GptRequest
{
    public string            model;
    public GptRequestMessage[] messages;
}

// ---------------------------------------------------------------
// レスポンス用ラッパークラス
// choices[0].message.content のネスト構造に対応（04-platform-gotchas §3）
// ---------------------------------------------------------------

[Serializable]
internal class GptMessage
{
    public string content;
}

[Serializable]
internal class GptChoice
{
    public GptMessage message;
}

[Serializable]
internal class GptResponse
{
    public GptChoice[] choices;
}

/// <summary>
/// ILanguageModelService の外部 API 実装。
/// OpenAI Chat Completions API（gpt-4o-mini）に JSON で送信し回答テキストを取得する。
/// config.systemPrompt を system メッセージとして必ず渡す（読み飛ばし禁止・04-platform-gotchas §3）。
/// Stateless 設計（1問1答）。会話履歴は将来拡張扱い（§10.9）。
/// </summary>
public class ExternalLanguageModelClient : ILanguageModelService
{
    private const string Endpoint = "https://api.openai.com/v1/chat/completions";
    private const string Model    = "gpt-4o-mini";

    private readonly string _apiKey;
    private readonly string _systemPrompt;

    public ExternalLanguageModelClient(AppConfig config)
    {
        _apiKey       = config.openAIApiKey;
        _systemPrompt = config.systemPrompt;
    }

    public async UniTask<string> GenerateResponseAsync(string userMessage)
    {
        // リクエストボディを組み立てる
        var requestBody = new GptRequest
        {
            model = Model,
            messages = new[]
            {
                new GptRequestMessage { role = "system", content = _systemPrompt }, // 省略禁止
                new GptRequestMessage { role = "user",   content = userMessage   }
            }
        };

        string json      = JsonUtility.ToJson(requestBody);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

        using var www = new UnityWebRequest(Endpoint, "POST");
        www.uploadHandler   = new UploadHandlerRaw(bodyBytes);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type",  "application/json");
        www.SetRequestHeader("Authorization", $"Bearer {_apiKey}");

        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            throw new Exception($"[LLM] API通信失敗: {www.error} / {www.downloadHandler.text}");

        var response = JsonUtility.FromJson<GptResponse>(www.downloadHandler.text);
        return response.choices[0].message.content;
    }
}
