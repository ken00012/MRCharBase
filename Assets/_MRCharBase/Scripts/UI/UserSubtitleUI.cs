// 配置: World Space Canvas 内の UserSubtitle GameObject（§9 UI/ フォルダ準拠）
// 責務: STT で取得したユーザー発話テキストの表示・クリアのみを担う（AIの字幕は XRInteractionUI が担当）

using TMPro;
using UnityEngine;

/// <summary>
/// ユーザーの発話テキスト（STT 結果）を表示する専用 UI コンポーネント。
/// CharacterStateController から直接呼ばれる。
/// AIの字幕（XRInteractionUI）とは独立したコンポーネントとして設計し、
/// 将来の非表示化・スタイル変更を単独で行えるようにする。
/// </summary>
public class UserSubtitleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text userSubtitleText;
    [SerializeField] private Color    userTextColor = Color.cyan; // AIの字幕（白）と区別するため色を分ける

    /// <summary>
    /// ユーザーの発話テキスト（STT 結果）を表示する。
    /// CharacterStateController.RunPipelineAsync() 内、TranscribeAsync 成功直後に呼ばれる。
    /// </summary>
    public void ShowUserSubtitle(string text)
    {
        if (userSubtitleText == null) return;
        userSubtitleText.color = userTextColor;
        userSubtitleText.text  = text;
    }

    /// <summary>
    /// テキストをクリアする。
    /// CharacterStateController.OnRecordButtonTapped() の Idle→Listening ブランチで呼ばれる。
    /// </summary>
    public void Clear()
    {
        if (userSubtitleText == null) return;
        userSubtitleText.text = string.Empty;
    }
}
