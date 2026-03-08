// 配置: Assets/_MRCharBase/Scripts/App/
// 責務: config.json のデータ構造を定義するデータクラス（§10.1）

using System;

/// <summary>
/// StreamingAssets/config.json の構造に対応するデータクラス。
/// JsonUtility.FromJson&lt;AppConfig&gt;() で直接デシリアライズされる。
/// フィールド名は config.json のキー名と完全一致させること。
/// </summary>
[Serializable]
public class AppConfig
{
    public string openAIApiKey;
    public string elevenLabsApiKey;
    public string elevenLabsVoiceId;
    public string systemPrompt;
}
