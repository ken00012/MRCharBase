// 配置: UICanvas/RecordButton GameObject
// 責務: ON/OFF状態色とPointer Hover/Press色を競合なしに合成し Image に適用する
//       XRInteractionUI.UpdateButtonColor() から SetState() を呼ぶ。
//       Button.Transition = None と組み合わせて使用すること。

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ボタンの「状態色（ON/OFF）」と「インタラクション色（Hover/Press）」を
/// 競合なく合成して Image.color に適用する MonoBehaviour。
///
/// PointableCanvasModule が Ray/Poke 両入力を IPointerXxxHandler に変換するため、
/// このスクリプトは Ray・Poke どちらの入力でも正しく反応する。
///
/// 注意: Button コンポーネントの Transition は必ず None に設定すること。
///       Image.color への書き込みはこのスクリプトのみが行う（一元管理）。
/// </summary>
[RequireComponent(typeof(Image))]
public class ButtonStateColorController : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler,  IPointerUpHandler
{
    [Header("OFF状態（待機中）の色")]
    [SerializeField] private Color _offNormal  = new Color(0.86f, 0.23f, 0.23f, 1f); // 赤（既存 idleColor に合わせた初期値）
    [SerializeField] private Color _offHover   = new Color(1.00f, 0.40f, 0.40f, 1f);
    [SerializeField] private Color _offPressed = new Color(0.65f, 0.15f, 0.15f, 1f);

    [Header("ON状態（録音中）の色")]
    [SerializeField] private Color _onNormal   = new Color(0.23f, 0.72f, 0.34f, 1f); // 緑（既存 listeningColor に合わせた初期値）
    [SerializeField] private Color _onHover    = new Color(0.40f, 0.90f, 0.50f, 1f);
    [SerializeField] private Color _onPressed  = new Color(0.15f, 0.55f, 0.25f, 1f);

    [Header("フェード時間（秒）")]
    [SerializeField] private float _fadeDuration = 0.1f;

    private Image     _image;
    private bool      _isOn;
    private bool      _isHovered;
    private bool      _isPressed;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _image = GetComponent<Image>();
        ApplyColor(instant: true); // 初期色を即時適用
    }

    // ─── 外部API ──────────────────────────────────────────────────────────────

    /// <summary>
    /// XRInteractionUI.UpdateButtonColor() から呼ぶ。
    /// 録音中(true) / 待機中(false) を渡すと、現在のHover状態を保ったまま色を切り替える。
    /// </summary>
    public void SetState(bool isOn)
    {
        _isOn = isOn;
        ApplyColor();
    }

    // ─── IPointerXxxHandler ───────────────────────────────────────────────────
    // PointableCanvasModule が Ray/Poke 両入力をこれらのインターフェースに変換して届ける

    public void OnPointerEnter(PointerEventData _) { _isHovered = true;  ApplyColor(); }
    public void OnPointerExit(PointerEventData _)  { _isHovered = false; _isPressed = false; ApplyColor(); }
    public void OnPointerDown(PointerEventData _)  { _isPressed = true;  ApplyColor(); }
    public void OnPointerUp(PointerEventData _)    { _isPressed = false; ApplyColor(); }

    // ─── 色合成ロジック ───────────────────────────────────────────────────────

    /// <summary>
    /// ON/OFF × Hover/Press の組み合わせから目標色を決定する。
    /// Press > Hover > Normal の優先順位。
    /// </summary>
    private Color GetTargetColor()
    {
        if (_isPressed) return _isOn ? _onPressed : _offPressed;
        if (_isHovered) return _isOn ? _onHover   : _offHover;
        return                 _isOn ? _onNormal   : _offNormal;
    }

    private void ApplyColor(bool instant = false)
    {
        Color target = GetTargetColor();

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        if (instant || _fadeDuration <= 0f)
        {
            _image.color = target;
            return;
        }

        _fadeCoroutine = StartCoroutine(FadeToColor(target));
    }

    private IEnumerator FadeToColor(Color target)
    {
        Color start   = _image.color;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed      += Time.deltaTime;
            _image.color  = Color.Lerp(start, target, elapsed / _fadeDuration);
            yield return null;
        }

        _image.color   = target;
        _fadeCoroutine = null;
    }
}
