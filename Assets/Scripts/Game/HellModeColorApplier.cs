using UnityEngine;
using UnityEngine.UI;
using BomBomLemon.PlayerSetup;

namespace BomBomLemon.Game
{
    /// <summary>
    /// 地獄モード時にゲーム画面の背景・ボタン色をライムグリーンパレットへ変更。
    /// Awake で即適用するため視覚的なちらつきなし。
    /// </summary>
    public class HellModeColorApplier : MonoBehaviour
    {
        [SerializeField] Camera  mainCamera;
        [SerializeField] Image   backgroundImage;
        [SerializeField] Image[] ctaButtonImages;

        // タイトル地獄モードと同一カラー
        static readonly Color BgHell  = new Color(0.52f, 0.76f, 0.32f, 1f);
        static readonly Color BtnHell = new Color(0.38f, 0.70f, 0.25f, 0.90f);

        void Awake()
        {
            if (!SinglePlayConfig.IsHellMode) return;
            if (mainCamera)      mainCamera.backgroundColor = BgHell;
            if (backgroundImage) backgroundImage.color      = BgHell;
            if (ctaButtonImages != null)
                foreach (var img in ctaButtonImages)
                    if (img) img.color = BtnHell;
        }
    }
}
