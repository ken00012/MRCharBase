// 配置: Assets/_MRCharBase/Scripts/Services/Mock/
// 責務: LLM Mock 実装（ネットワーク不要で全体フロー確認用）（§10.9・02-architecture §7）

using Cysharp.Threading.Tasks;

/// <summary>
/// ILanguageModelService の Mock 実装。
/// 遅延を含めることで状態遷移テストが有効になる（遅延なし不可・02-architecture §7）。
/// useMock = true 時に AppSetup から注入される。
/// </summary>
public class MockLanguageModelService : ILanguageModelService
{
    public async UniTask<string> GenerateResponseAsync(string userMessage)
    {
        await UniTask.Delay(2000); // 遅延必須（省略禁止）
        return "これはMockの回答です。質問を受け付けました。";
    }
}
