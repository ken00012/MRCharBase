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
    // ── OpenAI ──────────────────────────────────────────────────
    public string openAIApiKey;         // STT (Whisper) / LLM (GPT) / TTS (OpenAI) で共用

    // ── ElevenLabs TTS ──────────────────────────────────────────
    public string elevenLabsApiKey;
    public string elevenLabsVoiceId;

    // ── Fish Audio TTS ───────────────────────────────────────────
    public string fishAudioApiKey;
    public string fishAudioReferenceId;
    public string fishAudioModel;       // 空文字の場合は ExternalFishAudioClient 側で "s2-pro" にフォールバック

    // ── Google Cloud TTS ─────────────────────────────────────────
    public string googleTtsApiKey;
    public string googleTtsLanguageCode; // 空文字の場合は ExternalGoogleTtsClient 側で "ja-JP" にフォールバック
    public string googleTtsVoiceName;    // 空文字の場合は ExternalGoogleTtsClient 側で "ja-JP-Neural2-B" にフォールバック

    // ── OpenAI TTS ───────────────────────────────────────────────
    public string openAiTtsModel;       // 空文字の場合は ExternalOpenAiTtsClient 側で "tts-1" にフォールバック
    public string openAiTtsVoice;       // 空文字の場合は ExternalOpenAiTtsClient 側で "alloy" にフォールバック

    // ── LLM ─────────────────────────────────────────────────────
    public string systemPrompt;
}
