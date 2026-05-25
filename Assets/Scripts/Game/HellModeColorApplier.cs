using UnityEngine;
using UnityEngine.UI;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Game
{
    /// <summary>
    /// 地獄モード時に背景のレモン透かしパターンをライムスプライトへ差し替える。
    /// それ以外の色・ボタン・UIは通常モードと完全に同一。
    /// </summary>
    public class HellModeColorApplier : MonoBehaviour
    {
        [SerializeField] Transform lemonPatternRoot;
        [SerializeField] Sprite    limeSprite;

        void Awake()
        {
            if (!SinglePlayConfig.IsHellMode) return;

            // レモン透かし → ライム差し替え
            if (lemonPatternRoot != null && limeSprite != null)
                foreach (var img in lemonPatternRoot.GetComponentsInChildren<Image>(true))
                    img.sprite = limeSprite;
        }
    }
}
