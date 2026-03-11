// 配置: キャラクター Root GameObject
// 責務: 状態管理 / 音声AIパイプライン制御（§10.4）

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// キャラクターの状態管理と STT→LLM→TTS パイプライン制御を一元管理する MonoBehaviour。
/// 状態変更は必ず SetState() 経由で行う（_state への直接代入禁止）。
/// サービスは AppSetup.InitAsync() から Inject() メソッド経由で注入される。
/// </summary>
public class CharacterStateController : MonoBehaviour
{
    [SerializeField] private XRInteractionUI  ui;
    [SerializeField] private TeacherPresenter presenter;

    private ISpeechToTextService  _sttService;
    private ILanguageModelService _llmService;
    private ITextToSpeechService  _ttsService;
    private ISpatialAudioPlayer   _audioPlayer;
    private IAudioRecorder        _recorder;    // AppSetup から Inject される
    private CharacterState        _state;

    // ★ プロトタイプでは未使用。将来、処理中キャンセルボタンを追加する際に
    //   _cts.Cancel() を呼ぶ。現在は RunPipelineAsync() で生成のみ行う。
    //   実際にキャンセルを有効にするには、TranscribeAsync / GenerateResponseAsync /
    //   SynthesizeAsync の各インターフェースに CancellationToken を引数として追加し、
    //   _cts.Token を渡す必要がある（インターフェース変更が伴う将来拡張）。
    private CancellationTokenSource _cts;

    /// <summary>AppSetup から呼ばれる（依存性注入）</summary>
    public void Inject(
        ISpeechToTextService  stt,
        ILanguageModelService llm,
        ITextToSpeechService  tts,
        ISpatialAudioPlayer   audio,
        IAudioRecorder        recorder)
    {
        _sttService  = stt;
        _llmService  = llm;
        _ttsService  = tts;
        _audioPlayer = audio;
        _recorder    = recorder;
    }

    // PokeInteractable の WhenSelect() イベントに接続
    public void OnRecordButtonTapped()
    {
        if (_recorder == null) return; // Inject 未完了ガード

        if (_state == CharacterState.Idle)
        {
            // 待機中 → 録音開始
            SetState(CharacterState.Listening);
            ui.ShowSubtitle("録音中... もう一度タップで送信");
            _recorder.StartRecording();
        }
        else if (_state == CharacterState.Listening)
        {
            // 録音中 → 録音停止してパイプライン実行
            byte[] wavData = _recorder.StopRecording();
            RunPipelineAsync(wavData).Forget();
        }
        // Thinking / Speaking 中のタップは無視（二重実行防止）
    }

    private async UniTaskVoid RunPipelineAsync(byte[] wavData)
    {
        _cts = new CancellationTokenSource(); // ⓪ 先頭1行目（省略禁止）

        SetState(CharacterState.Thinking);
        ui.ShowSubtitle("AIが考えています..."); // Thinking 中フィードバック（必須）

        // 録音データが空の場合は API に送らず Idle に戻す（マイク未接続・録音直後停止対策）
        if (wavData == null || wavData.Length == 0)
        {
            ui.ShowError("録音データが取得できませんでした。");
            SetState(CharacterState.Idle);
            return;
        }

        string question;
        try { question = await _sttService.TranscribeAsync(wavData); }
        catch (Exception e)
        {
            Debug.LogError($"[STT] {e.Message}");
            ui.ShowError("音声認識に失敗しました。");
            SetState(CharacterState.Idle);
            return;
        }

        await RunFromLLMAsync(question); // STT 後は LLM 以降の共通処理へ
    }

    /// <summary>
    /// デバッグ用: STT をスキップしてテキストを直接 LLM に送る。
    /// Editor でボタンに割り当ててネットワーク不要で LLM+TTS+再生を確認できる。
    /// ※ UniTaskVoid は呼び出し元が完了を待てない。デバッグ専用のため許容。
    ///   将来、テスト等で完了を待ちたい場合は UniTask に変更すること。
    /// </summary>
    public async UniTaskVoid OnDebugInput(string question)
    {
        if (string.IsNullOrEmpty(question)) return; // 空入力は無視（省略禁止）
        try
        {
            SetState(CharacterState.Thinking);
            ui.ShowSubtitle("AIが考えています...");
            await RunFromLLMAsync(question); // STT をスキップ
        }
        catch (Exception e)
        {
            Debug.LogError($"[DebugInput] {e.Message}");
            ui.ShowError("デバッグ入力でエラーが発生しました。");
            SetState(CharacterState.Idle);
        }
    }

    // デバッグ用ラッパー（引数なし・Inspectorの3点メニューから呼び出し可能）
    // Use_Mock = Falseのときに、LLMへのインプットとしてエディタ上で使えるかも...
    // Use_Mock = Trueのときは、MockSpeechToTextServiceがよばれ、そのテキストが疑似的に処理される
    [SerializeField] private string _debugText = "debug用テキスト";

    [ContextMenu("Debug: OnDebugInput")]
    public void OnDebugInputFromMenu()
    {
        OnDebugInput(_debugText).Forget();
    }

    /// <summary>LLM→TTS→再生の共通処理（RunPipelineAsync・OnDebugInput 両方から呼ぶ）</summary>
    private async UniTask RunFromLLMAsync(string question)
    {
        string answer;
        try
        {
            answer = await _llmService.GenerateResponseAsync(question);
            ui.ShowSubtitle(answer); // LLM try ブロック内・GenerateResponseAsync 直後（省略禁止）
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLM] {e.Message}");
            ui.ShowError("AI回答の取得に失敗しました。");
            SetState(CharacterState.Idle);
            return;
        }

        try
        {
            SetState(CharacterState.Speaking); // SynthesizeAsync の前に Speaking へ
            AudioClip clip = await _ttsService.SynthesizeAsync(answer);
            await _audioPlayer.PlayAsync(clip);
        }
        catch (Exception e)
        {
            Debug.LogError($"[TTS] {e.Message}");
            ui.ShowError("音声合成に失敗しました。");
            // SetState(Idle) はここに書かない → finally に委ねる（03_ai_guard.md 準拠）
        }
        finally { SetState(CharacterState.Idle); } // 正常・異常どちらも必ず Idle に戻す
    }

    private void SetState(CharacterState newState)
    {
        _state = newState;
        presenter.OnStateChanged(newState); // TeacherPresenter への通知（省略禁止）
        ui.UpdateButtonColor(newState); // ← 追加
    }

    /// <summary>AppSetup から設定読み込み失敗時に呼ばれる致命的エラー表示</summary>
    public void ShowFatalError(string message) => ui.ShowError(message);

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
