# MRCharBase

**Meta Quest 3 向け MR AIキャラクター対話基盤プロトタイプ**

MR（Mixed Reality）空間に表示されたVRMキャラクターに音声で話しかけると、AIが回答を生成してキャラクターの声で返答します。  
学生がXRアプリ開発を学ぶためのシンプルで拡張しやすいベースプロトタイプです。

---

## Features

- 🎤 **リアルタイム音声認識 (STT)**: OpenAI Whisper API によるマイク入力の文字起こし
- 💬 **AI対話生成 (LLM)**: OpenAI GPT-4o-mini による自然な回答生成（1問1答 Stateless 設計）
- 🔊 **複数TTSエンジンの動的切り替え**: Inspector から4種類を即座に切り替え可能
  | TTS エンジン | 特徴 |
  |---|---|
  | **ElevenLabs** | 高品質英語・日本語音声 |
  | **Fish Audio** | カスタム参照音声クローン対応 |
  | **Google Cloud TTS** | 安定した日本語音声（Neural2推奨） |
  | **OpenAI TTS** | シンプルな統合（openAIApiKey で共用） |
- 📝 **ユーザー発話字幕UI**: STT結果をリアルタイムにシアン色で表示（`UserSubtitleUI`）
- 🥽 **Meta Quest 3 MRパススルー対応**: PokeInteractable ボタン（指タッチ操作）
- 🧪 **Mock モード**: APIキー不要でエディタ上の全フローを即座に確認
- 🏗️ **依存性注入（DI）設計**: インターフェース経由でサービスを差し替え可能な拡張しやすい構造

---

## Requirements

| 項目 | バージョン / 内容 |
|---|---|
| Unity | **6000.0.60f1** |
| レンダリングパイプライン | Universal Render Pipeline (URP) |
| XR SDK | Meta XR All-in-One SDK（最新安定版） |
| VRMローダー | UniVRM **0.104.0** |
| 非同期ライブラリ | UniTask（最新安定版） |
| ターゲットデバイス | Meta Quest 3 |
| ビルドターゲット | Android |

---

## Getting Started

### 1. リポジトリをクローン

```bash
git clone https://github.com/ken00012/MRCharBase.git
```

### 2. Unity でプロジェクトを開く

Unity Hub から **Unity 6000.0.60f1** でプロジェクトを開いてください。

### 3. 必要パッケージをインポート

Unity Package Manager (UPM) または各公式ページからインポートしてください：

- **Meta XR All-in-One SDK** — [Meta Developer](https://developer.oculus.com/downloads/package/meta-xr-sdk-all-in-one-upm/)
- **UniVRM 0.104.0** — [GitHub Releases](https://github.com/vrm-c/UniVRM/releases/tag/v0.104.0)
- **UniTask** — [GitHub](https://github.com/Cysharp/UniTask) → UPM で `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`

### 4. VRM モデルの配置

```
Assets/
└── _MRCharBase/
    └── VRM/
        └── teacher.vrm   ← ここに配置
```

### 5. `StreamingAssets/config.json` を作成

> [!CAUTION]
> **`config.json` はリポジトリにコミットしないでください。**  
> このファイルには API キーが含まれます。必ず `.gitignore` に追加してください（プロジェクトには設定済み）。

`Assets/StreamingAssets/config.json` を以下のテンプレートで作成してください。

```json
{
    "openAIApiKey": "sk-xxxxxxxxxxxxxxxxxxxx",

    "elevenLabsApiKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "elevenLabsVoiceId": "xxxxxxxxxxxxxxxxxxxxxxxx",

    "fishAudioApiKey": "YOUR_FISH_AUDIO_API_KEY",
    "fishAudioReferenceId": "YOUR_REFERENCE_ID",
    "fishAudioModel": "s2-pro",

    "googleTtsApiKey": "YOUR_GOOGLE_TTS_API_KEY",
    "googleTtsLanguageCode": "ja-JP",
    "googleTtsVoiceName": "ja-JP-Neural2-B",

    "openAiTtsModel": "tts-1",
    "openAiTtsVoice": "alloy",

    "systemPrompt": "あなたは大学の先生です。学生に優しく丁寧に説明してください。回答は100字程度の平文でシンプルに分かりやすくしてください。"
}
```

**設定のポイント：**
- 使用しない TTS エンジンのフィールドは省略可能です
- フォールバック値: `fishAudioModel` → `"s2-pro"` / `googleTtsLanguageCode` → `"ja-JP"` / `openAiTtsModel` → `"tts-1"` / `openAiTtsVoice` → `"alloy"`

### 6. 日本語フォント設定

NotoSansJP-MediumをTextMeshProでSDFに変換する必要あり。
FontAssetCreatorでSDFに変換すること。

### 7. TTS エンジンを選択

Unity Hierarchy で `AppSetup` GameObject を選択し、Inspector の `Tts Engine` フィールドで使用するエンジンを選択してください。

| Inspector 値 | 使用クライアント | 必要なキー |
|---|---|---|
| `ElevenLabs` | ExternalElevenLabsClient | `elevenLabsApiKey`, `elevenLabsVoiceId` |
| `FishAudio` | ExternalFishAudioClient | `fishAudioApiKey`, `fishAudioReferenceId` |
| `Google` | ExternalGoogleTtsClient | `googleTtsApiKey` |
| `OpenAI` | ExternalOpenAiTtsClient | `openAIApiKey` |

### 8. Mock モードで動作確認（済）

**APIキーなしで全フローを確認する方法：**

1. `AppSetup` の `Use Mock` を `true` に設定
2. Unity エディタで Play
3. `CharacterStateController` を選択 → Inspector 右上の「⋮」→「Debug: OnDebugInput」を実行
4. STT をスキップして LLM → TTS → 音声再生の全フローを確認できます

### 9. Quest3 実機ビルド

1. `File → Build Settings` で Platform を `Android` に切り替え（確認）
2. `Player Settings` で XR 設定を確認（Meta XR SDK 設定済み）
3. Quest3 を USB 接続して `Build`
4. 初回起動時のマイク権限・空間認識ダイアログで「許可」を選択

---

## Project Structure

```
Assets/_MRCharBase/Scripts/
├── App/            ← 起動・設定・DI（AppSetup, AppConfig, LocalConfigProvider）
├── Core/           ← 中心制御（CharacterStateController, CharacterState）
├── Services/
│   ├── Interfaces/ ← サービス抽象化（ISpeechToTextService 等）
│   ├── External/   ← API クライアント実装（ElevenLabs / Fish Audio / Google / OpenAI TTS）
│   └── Mock/       ← テスト用 Mock 実装
├── Voice/          ← 音声入力（VoiceInputHandler, UnityMicrophoneRecorder, WavUtility）
├── Audio/          ← 音声出力（SpatialAudioPlayer）
├── UI/             ← XR UI（XRInteractionUI, UserSubtitleUI）
└── Character/      ← キャラクター制御（TeacherPresenter）

StreamingAssets/
└── config.json
```

詳細なアーキテクチャ・クラス設計は [`prototype_design.md`](./prototype_design.md) を参照してください。

---

## Architecture Overview

```
AppSetup（Composition Root）
  ├─ config.json を読込（LocalConfigProvider）
  ├─ TtsEngine で選択した TTS クライアントを生成
  └─ CharacterStateController に全サービスを注入（DI）

CharacterStateController（中心制御）
  ├─ Idle → [ボタンタップ] → Listening → [ボタンタップ] → Thinking
  ├─ STT（Whisper）→ UserSubtitleUI にユーザー発話を表示
  ├─ LLM（GPT-4o-mini）→ XRInteractionUI にAI回答を表示
  ├─ TTS（選択エンジン）→ SpatialAudioPlayer で音声再生
  └─ Idle に戻る
```

---

## Notes

- **APIキー管理**: 本プロトタイプは教育目的のため Unity クライアントから直接 API 通信します。実運用では中間サーバーを設置してください。
- **会話履歴**: 現在の LLM は 1問1答 の Stateless 設計です。文脈の引き継ぎは将来拡張として設計されています。
- **マイク権限**: Quest3 実機の初回起動時、権限ダイアログで「許可」を選択してから操作してください。
- **RAG（知識ベース）の追加**: 独自のPDFや社内ドキュメント等を読み込ませるRAGを実装したい場合は、`ILanguageModelService` の実装クラスを新規追加し、AppSetupで注入するだけで既存のロジックを壊さずに拡張可能です。
- **リップシンクとアニメーション**: キャラクターの口の動きや感情表現（身振り手振り）を追加したい場合は、表示の責務を持つ `TeacherPresenter.cs` のみを拡張することで安全に実装できます。

---

*Based on: prototype_design.md v5.0 | Unity 6000.0.60f1 | Meta Quest 3*
