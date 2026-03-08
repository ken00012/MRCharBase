// 配置: Assets/_MRCharBase/Scripts/Services/Interfaces/
// 責務: 設定読込の抽象化（§10.1）

using Cysharp.Threading.Tasks;

/// <summary>
/// 設定読込サービスのインターフェース。
/// 実装: LocalConfigProvider（StreamingAssets/config.json 読込）
/// 将来: クラウド設定・環境別設定への展開に対応可能（§10.1）。
/// Android StreamingAssets は非同期読込必須のため async にする。
/// </summary>
public interface IConfigProvider
{
    UniTask<AppConfig> LoadAsync();
}
