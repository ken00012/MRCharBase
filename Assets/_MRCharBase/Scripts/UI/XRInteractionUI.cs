// 配置: UICanvas root GameObject（§10.8・§13 準拠 — RecordButton 配下から UICanvas root へ移動）
// 責務: 字幕 / エラー / 状態表示。ボタン色は ButtonStateColorController に委譲する。
//       ボタンイベント発火元は Button.OnClick（PointableCanvasModule 経由）。

using TMPro;
using UnityEngine;

/// <summary>
/// XR UI の字幕・エラー表示、およびボタン状態の通知を担当する MonoBehaviour。
/// World Space Canvas root に配置する（Meta XR Interaction SDK 対応）。
///
/// ShowError() は内部で "[Error] {message}" を自動付加する。
/// 呼び出し側で "[Error] " を手動付加しないこと（二重になる）。
///
/// ボタンの Image.color 管理は ButtonStateColorController に完全委譲する。
/// このクラスは色値を直接持たない。
/// </summary>
public class XRInteractionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text                   subtitleText;
    [SerializeField] private TMP_Text                   buttonLabel;
    [SerializeField] private ButtonStateColorController _buttonColor; // RecordButtonをアサイン
    [SerializeField] private Color normalColor = Color.black;
    [SerializeField] private Color errorColor  = Color.red;

    /// <summary>通常の字幕テキストを表示する。</summary>
    public void ShowSubtitle(string text)
    {
        subtitleText.color = normalColor;
        subtitleText.text  = text;
    }

    /// <summary>エラーメッセージを赤色で表示する。</summary>
    public void ShowError(string message)
    {
        subtitleText.color = errorColor;
        subtitleText.text  = $"[Error] {message}";
    }

    /// <summary>字幕テキストをクリアする。</summary>
    public void Clear() => subtitleText.text = string.Empty;

    /// <summary>
    /// CharacterStateController.SetState() から呼ぶ。
    /// ボタン色の制御は ButtonStateColorController に委譲し、
    /// このメソッドはラベルテキストの更新のみ行う。
    /// </summary>
    public void UpdateButtonColor(CharacterState state)
    {
        // 色の管理を ButtonStateColorController に委譲（Image.color の直接書き込みなし）
        _buttonColor?.SetState(state == CharacterState.Listening);

        // ボタンラベルのテキスト更新（Listening = ON, それ以外 = OFF）
        // Thinking / Speaking 中はボタン操作が無視されるため OFF 表示で統一
        if (buttonLabel != null)
            buttonLabel.text = state == CharacterState.Listening ? "ON" : "OFF";
    }
}
