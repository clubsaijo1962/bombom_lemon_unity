using UnityEngine;
using UnityEngine.UI;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Game
{
    /// <summary>
    /// 地獄モード時にゲーム画面の背景をグリーンに変更し、
    /// レモン透かしパターンをライムスプライトへ差し替える。
    /// ボタン・テキスト・その他UIは通常モードと同一。
    /// </summary>
    public class HellModeColorApplier : MonoBehaviour
    {
        [SerializeField] Camera    mainCamera;
        [SerializeField] Image     backgroundImage;
        [SerializeField] Transform lemonPatternRoot;
        [SerializeField] Sprite    limeSprite;

        // タイトル地獄モードと同一カラー
        static readonly Color BgHell = new Color(0.52f, 0.76f, 0.32f, 1f);

        void Awake()
        {
            if (!SinglePlayConfig.IsHellMode) return;

            // 背景を緑に（カメラ背景色 + UIパネル）
            if (mainCamera)      mainCamera.backgroundColor = BgHell;
            if (backgroundImage) backgroundImage.color      = BgHell;

            // レモン透かし → ライム差し替え
            if (lemonPatternRoot != null && limeSprite != null)
                foreach (var img in lemonPatternRoot.GetComponentsInChildren<Image>(true))
                    img.sprite = limeSprite;
        }
    }
}
