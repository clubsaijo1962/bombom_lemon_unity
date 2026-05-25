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

        // タイトル地獄モードと同一カラー（薄いパステルグリーン）
        static readonly Color BgHell = new Color(0.76f, 0.93f, 0.56f, 1f);

        void Awake()
        {
            if (!SinglePlayConfig.IsHellMode) return;

            // 背景を薄いグリーンに（カメラ背景色 + UIパネル）
            if (mainCamera)      mainCamera.backgroundColor = BgHell;
            if (backgroundImage) backgroundImage.color      = BgHell;

            // レモン透かし → ライム差し替え＋透かしを少し濃くして視認性向上
            if (lemonPatternRoot != null && limeSprite != null)
                foreach (var img in lemonPatternRoot.GetComponentsInChildren<Image>(true))
                {
                    img.sprite = limeSprite;
                    img.color  = new Color(1f, 1f, 1f, 0.13f); // 通常0.07 → 0.13
                }
        }
    }
}
