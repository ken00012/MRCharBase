// 配置: Assets/_MRCharBase/Scripts/Services/Interfaces/
// 責務: LLM（回答生成）サービスの抽象化（§10.1）

using Cysharp.Threading.Tasks;

/// <summary>
/// LLM（言語モデル）サービスのインターフェース。
/// 実装: ExternalLanguageModelClient（本番）/ MockLanguageModelService（Mock）
/// Stateless（1問1答）設計。会話履歴は将来拡張扱い（§10.9）。
/// </summary>
public interface ILanguageModelService
{
    UniTask<string> GenerateResponseAsync(string userMessage);
}
