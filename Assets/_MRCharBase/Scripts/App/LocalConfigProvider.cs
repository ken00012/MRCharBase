// 配置: Assets/_MRCharBase/Scripts/App/
// 責務: config.json を StreamingAssets から非同期読み込みする（§10.2・IConfigProvider実装）

using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// StreamingAssets/config.json を UnityWebRequest で非同期読み込みする。
/// Android / Quest 実機では File.ReadAllText() が動作しないため、
/// UnityWebRequest を使用する（§10.2・04-platform-gotchas §1）。
/// </summary>
public class LocalConfigProvider : IConfigProvider
{
    public async UniTask<AppConfig> LoadAsync()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "config.json");
        using var www = UnityWebRequest.Get(path);
        await www.SendWebRequest();

        // 成功チェック必須（失敗時は例外で呼び出し元に伝える）
        if (www.result != UnityWebRequest.Result.Success)
            throw new Exception($"[Config] 読込失敗: {www.error}");

        return JsonUtility.FromJson<AppConfig>(www.downloadHandler.text);
    }
}
