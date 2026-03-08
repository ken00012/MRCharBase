// 配置: Assets/_MRCharBase/Scripts/Services/Interfaces/
// 責務: 空間音響再生サービスの抽象化（§10.1）

using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 空間音響再生サービスのインターフェース。
/// 実装: SpatialAudioPlayer（MonoBehaviour）
/// useMock に関わらず常にMonoBehaviour実装を使用する（§10.1・01-always-on.md D項）。
/// </summary>
public interface ISpatialAudioPlayer
{
    UniTask PlayAsync(AudioClip clip); // 再生完了まで待機
}
