// 配置: World Space Canvas 内の RecordButton GameObject 配下（§10.8・§13 準拠）
// 責務: 字幕 / エラー / 状態表示（ボタンイベント発火元は InteractableUnityEventWrapper）

using TMPro;
using UnityEngine;

/// <summary>
/// XR UI の字幕・エラー表示を担当する MonoBehaviour。
/// World Space Canvas に配置する（Meta XR Interaction SDK 対応）。
/// ShowError() は内部で "[Error] {message}" を自動付加する。
/// 呼び出し側で "[Error] " を手動付加しないこと（二重になる）。
/// </summary>
public class XRInteractionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color errorColor  = Color.red;

    /// <summary>通常の字幕テキストを表示する。</summary>
    public void ShowSubtitle(string text)
    {
        subtitleText.color = normalColor;
        subtitleText.text  = text;
    }

    /// <summary>
    /// エラーメッセージを赤色で表示する。
    /// 内部で "[Error] " を自動付加するため、呼び出し側では付加しないこと。
    /// </summary>
    public void ShowError(string message)
    {
        subtitleText.color = errorColor;
        subtitleText.text  = $"[Error] {message}";
    }

    /// <summary>字幕テキストをクリアする。</summary>
    public void Clear() => subtitleText.text = string.Empty;
}
