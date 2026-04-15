// 配置: シーンに1つだけの専用 GameObject
// 責務: 全サービスを生成し CharacterStateController に注入する Composition Root（§10.2）

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// アプリ起動時に全サービスを生成・注入する Composition Root。
/// シーンに1つだけ配置する。
/// 初期化は Awake() ではなく Start() で行う（§10.2・03_ai_guard.md 準拠）。
/// </summary>
public class AppSetup : MonoBehaviour
{
    [SerializeField] private CharacterStateController characterController;
    [SerializeField] private SpatialAudioPlayer       audioPlayer;
    [SerializeField] private VoiceInputHandler        voiceInput;  // IAudioRecorder 実装済み

    [SerializeField] private bool useMock = false; // 削除禁止（01_always_on.md D項）

    // TTS エンジン選択（Inspector で切り替え可能）
    public enum TtsEngine { ElevenLabs, FishAudio, Google }
    [SerializeField] private TtsEngine ttsEngine = TtsEngine.Google;

    private void Start() => InitAsync().Forget(); // 非同期初期化（Awake ではなく Start 使用）

    private async UniTaskVoid InitAsync()
    {
        // ① マイク権限リクエスト（省略禁止）
        // 非ブロッキング: ダイアログは非同期で表示される。
        // 権限取得完了を待つには Callback か Unity Permissions API が必要。
        // プロトタイプでは「起動後すぐに録音しない」前提で許容する（§10.2）。
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            Permission.RequestUserPermission(Permission.Microphone);

        // ② config.json 読み込み（失敗時は3点セットで終了）
        AppConfig config;
        try { config = await new LocalConfigProvider().LoadAsync(); }
        catch (Exception e)
        {
            Debug.LogError($"[AppSetup] 設定読込失敗: {e.Message}");
            if (characterController != null)                              // ← ?.演算子ではなく if チェック
                characterController.ShowFatalError("設定ファイルの読み込みに失敗しました。");
            return;
        }

        // ③ サービス生成・注入
        ISpeechToTextService  stt   = useMock ? new MockSpeechToTextService()  : new ExternalSpeechToTextClient(config);
        ILanguageModelService llm   = useMock ? new MockLanguageModelService() : new ExternalLanguageModelClient(config);
        ITextToSpeechService  tts   = useMock
            ? (ITextToSpeechService)new MockTextToSpeechService()
            : ttsEngine == TtsEngine.FishAudio
                ? new ExternalFishAudioClient(config)
                : ttsEngine == TtsEngine.Google
                    ? new ExternalGoogleTtsClient(config)
                    : new ExternalElevenLabsClient(config);
        ISpatialAudioPlayer   audio = audioPlayer; // MonoBehaviour は Inspector 経由 DI
        IAudioRecorder        rec   = voiceInput;  // VoiceInputHandler : IAudioRecorder

        characterController.Inject(stt, llm, tts, audio, rec);
    }
}
