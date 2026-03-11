// 配置: World Space Canvas 内の RecordButton GameObject 配下（§10.8・§13 準拠）
// 責務: 字幕 / エラー / 状態表示（ボタンイベント発火元は InteractableUnityEventWrapper）

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// XR UI の字幕・エラー表示を担当する MonoBehaviour。
/// World Space Canvas に配置する（Meta XR Interaction SDK 対応）。
/// ShowError() は内部で "[Error] {message}" を自動付加する。
/// 呼び出し側で "[Error] " を手動付加しないこと（二重になる）。
/// </summary>

public class XRInteractionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text   subtitleText;
    [SerializeField] private Image      recordButtonBackground; // ← 追加
    [SerializeField] private TMP_Text buttonLabel; // ← 追加
    [SerializeField] private Color normalColor  = Color.white;
    [SerializeField] private Color errorColor   = Color.red;
    [SerializeField] private Color idleColor    = new Color(0.86f, 0.23f, 0.23f); // 赤
    [SerializeField] private Color listeningColor = new Color(0.23f, 0.72f, 0.34f); // 緑（録音中）

    /// 通常の字幕テキストを表示する。
    public void ShowSubtitle(string text)
    {
        subtitleText.color = normalColor;
        subtitleText.text  = text;
    }

    /// エラーメッセージを赤色で表示する。
    public void ShowError(string message)
    {
        subtitleText.color = errorColor;
        subtitleText.text  = $"[Error] {message}";
    }

    /// 字幕テキストをクリアする。
    public void Clear() => subtitleText.text = string.Empty;

    // CharacterStateController.SetState() から呼ぶ
    public void UpdateButtonColor(CharacterState state)
    {
        if (recordButtonBackground != null)
        {
            recordButtonBackground.color = state == CharacterState.Listening
                ? listeningColor
                : idleColor;
            // Thinking / Speaking 中はボタン操作が無視されるため idleColor で統一
        }

        if (buttonLabel != null)
        {
            buttonLabel.text = state == CharacterState.Listening
                ? "ON"
                : "OFF";
        }
    }
}
