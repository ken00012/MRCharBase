# MR Mentor Prototype

> Meta Quest 3 上で動作する **MR 音声対話システム**のベースプロトタイプ。  
> 学生が発展的な XR アプリを開発するための出発点として設計されています。

---

## 概要

MR 空間に表示された先生キャラクター（VRM）に音声で質問すると、AI が回答を生成し、組み込まれた特定の音声合成された声で返答を再生できるアプリです。
※先生キャラクターはあくまで例であり、他のキャラクターでも代用可能。

| 項目 | 内容 |
|---|---|
| デバイス | Meta Quest 3 |
| Unity バージョン | **6000.0.60f1**（固定） |
| キャラクター | VRM 形式（UniVRM 0.104.0） |
| STT | OpenAI Whisper API |
| LLM | OpenAI GPT（gpt-4o-mini 推奨） |
| TTS | ElevenLabs |
| 非同期 | UniTask（`async void` 禁止） |
| UI モード | MR パススルー |

---

## システム構成

```
ユーザー音声
    ↓ マイク録音（VoiceInputHandler）
    ↓ WAV 変換（WavUtility）
Whisper API → 質問テキスト
    ↓
GPT API     → 回答テキスト
    ↓
ElevenLabs  → AudioClip（MP3）
    ↓ 空間音響再生（SpatialAudioPlayer）
先生キャラクターが話す
```

### キャラクターの状態遷移

```
Idle ──[ボタンタップ]──→ Listening ──[再度ボタンタップ]──→ Thinking ──[TTS取得]──→ Speaking
 ↑                        |                           |                        |
 └──────────────[エラー発生時 / 再生完了]──────────────────────────────────────┘
```

| 状態 | 意味 | UI 表示 |
|---|---|---|
| Idle | 待機中 | — |
| Listening | 録音中 | 「録音中...」 |
| Thinking | AI 処理中 | 「AI が考えています...」 |
| Speaking | 話し中 | 回答の字幕 |

---

## セットアップ手順

### 1. 前提環境

- Unity **6000.0.60f1**（他バージョン非推奨）
- Android Build Support（Unity Hub からインストール）
- Meta Quest 3 実機 + 開発者モード有効化

### 2. Unity パッケージのインポート

以下の順番でインポートしてください。

1. **Meta XR All-in-One SDK**（最新安定版）  
   → Unity Package Manager または [Meta Developer Hub](https://developers.meta.com/) から

2. **UniVRM 0.104.0**  
   → [UniVRM GitHub Releases](https://github.com/vrm-c/UniVRM/releases) から `.unitypackage` をダウンロード・インポート

3. **UniTask**（最新安定版）  
   → Package Manager → `Add package from git URL`:  
   ```
   https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
   ```

### 3. VRM モデルの配置

```
Assets/
└── _MRCharBase/
    └── VRM/
        └── teacher.vrm   ← ここに配置
```

### 4. API キーの設定

> [!CAUTION]
> **`config.json` はリポジトリにコミットしないでください。**  
> ファイル作成直後に `.gitignore` への追記を忘れずに。

`StreamingAssets/config.json` を作成し、以下の内容を記入します。

```json
{
  "openAIApiKey": "sk-xxxxxxxxxxxxxxxxxxxx",
  "elevenLabsApiKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "elevenLabsVoiceId": "xxxxxxxxxxxxxxxxxxxxxxxx",
  "systemPrompt": "あなたは大学の先生です。学生に優しく丁寧に説明してください。回答は簡潔で分かりやすくしてください。"
}
```

`.gitignore` に追記：

```
StreamingAssets/config.json
```

### 5. Inspector の配線

シーンを開いた後、以下を確認してください。

**AppSetup GameObject**
- `characterController` → シーン内の `CharacterStateController`
- `audioPlayer` → シーン内の `SpatialAudioPlayer`
- `voiceInput` → シーン内の `VoiceInputHandler`

**CharacterRoot GameObject**
- `ui` → シーン内の `XRInteractionUI`
- `presenter` → 同じ GameObject の `TeacherPresenter`

**Head/Mouth GameObject**
- `audioSource` → 同 GameObject の `AudioSource` コンポーネント

**RecordButton（InteractableUnityEventWrapper）**
- `WhenSelect()` → `CharacterStateController.OnRecordButtonTapped()`

**RecordButton（XRInteractionUI）**
- `recordButtonBackground` → `RecordButton > ButtonBackground` の Image
- `buttonLabel` → `RecordButton > ButtonLabel` の TextMeshPro

### 6. 日本語フォント設定

NotoSansJP-MediumをTextMeshProでSDFに変換する必要あり。
FontAssetCreatorでSDFに変換すること。

---

## 開発の進め方（ステップ別）

| ステップ | 内容 | 実機不要？ |
|---|---|---|
| Step 1 | Unity プロジェクト作成・XR SDK 設定 | Editor のみ |
| Step 2 | VRM キャラクター配置 | Editor のみ |
| Step 3 | **Mock モードで全体フロー確認**（`useMock = true`） | Editor のみ ✅ |
| Step 4 | 音声入力・PokeInteractable ボタン実装 | Editor のみ |
| Step 5 | API 接続（`useMock = false`・`config.json` 設定） | Editor のみ |
| Step 6 | **Quest 3 実機ビルド・動作確認** | 実機必須 |

### Mock モードについて

`AppSetup` の `useMock` を `true` にすると、API 通信なしで全体フローをテストできます。

```csharp
[SerializeField] private bool useMock = false; // ← true にする
```

STT・LLM・TTS がすべて Mock に切り替わり、実際の API キーなしで状態遷移・字幕表示・エラー表示を確認できます。

### デバッグ用テキスト入力

`CharacterStateController.OnDebugInput(string question)` を使うと、STT をスキップして直接 LLM に質問テキストを送れます。 Inspectorの三点メニュー から呼び出してください。

---

## フォルダ構成

```
Assets/
└── _MRCharBase/
    ├── Scripts/
    │   ├── App/        AppSetup, LocalConfigProvider, AppConfig
    │   ├── Core/       CharacterStateController, CharacterState
    │   ├── Services/
    │   │   ├── Interfaces/   ISpeechToTextService 等
    │   │   ├── External/     実際の API クライアント
    │   │   └── Mock/         Mock 実装（削除禁止）
    │   ├── Voice/      VoiceInputHandler, UnityMicrophoneRecorder, WavUtility
    │   ├── Audio/      SpatialAudioPlayer
    │   ├── UI/         XRInteractionUI
    │   └── Character/  TeacherPresenter
    ├── Scenes/
    ├── Prefabs/
    └── VRM/
StreamingAssets/
└── config.json   ← .gitignore に追加必須
```

---

## 注意事項・よくある罠

**Quest 実機でマイクが使えない場合**  
初回起動時にマイク権限ダイアログが表示されます。「許可」を押してから録音ボタンを操作してください。「拒否」した場合は `設定 → アプリ → 権限` から手動でマイクをオンにしてください。

**`File.ReadAllText()` は Android で使えない**  
`StreamingAssets` の読み込みは必ず `UnityWebRequest` を使ってください（`LocalConfigProvider` 参照）。

**MP3 受信は `DownloadHandlerAudioClip` を明示設定**  
`UnityWebRequestMultimedia.GetAudioClip()` では TTS の MP3 受信に失敗するケースがあります。

**`async void` は使用禁止**  
例外が握りつぶされます。必ず `async UniTaskVoid` + `.Forget()` を使ってください。

---

## 拡張ポイント

このプロトタイプはインターフェースで抽象化されており、1行の差し替えで機能を拡張できます。

| 拡張機能 | 変更箇所 |
|---|---|
| STT を Azure 等に差し替え | `AppSetup.cs` の1行 |
| RAG による知識ベース追加 | `ILanguageModelService` 実装クラスを追加 |
| リップシンク・アニメーション | `TeacherPresenter.cs` のみ |
| 会話履歴の保持 | `ExternalLanguageModelClient.cs` のみ |
| 中間 API サーバー経由に変更 | 各 Client の URL 定数1行 |

---

## 関連ドキュメント

- `prototype_design.md` — 詳細設計書（v4.4）

---

*設計書バージョン: prototype_design.md v4.4*
