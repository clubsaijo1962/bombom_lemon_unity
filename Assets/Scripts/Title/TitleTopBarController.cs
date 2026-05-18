using UnityEngine;
using UnityEngine.UI;

namespace BomBomLemon.Title
{
    public class TitleTopBarController : MonoBehaviour
    {
        [SerializeField] private Button         rulesButton;
        [SerializeField] private Button         topicsButton;
        [SerializeField] private Button         hellModeButton;
        [SerializeField] private Image          hellButtonBg;
        [SerializeField] private Image          backgroundImage;
        [SerializeField] private LemonRainEffect lemonRain;
        [SerializeField] private RawImage       startButtonImage;
        [SerializeField] private Texture2D      lemonTexture;
        [SerializeField] private Texture2D      limeTexture;
        [SerializeField] private Texture2D      startNormalTexture;
        [SerializeField] private Texture2D      startLimeTexture;

        public bool IsHellMode { get; private set; } = false;

        static readonly Color BtnLemon = new Color(0.98f, 0.90f, 0.42f, 0.85f);
        static readonly Color BtnLime  = new Color(0.38f, 0.70f, 0.25f, 0.90f);
        static readonly Color BgNormal = new Color(0.98f, 0.90f, 0.55f, 1f);
        static readonly Color BgHell   = new Color(0.52f, 0.76f, 0.32f, 1f);

        void Start()
        {
            rulesButton?.onClick.AddListener(OnRules);
            topicsButton?.onClick.AddListener(OnTopics);
            hellModeButton?.onClick.AddListener(OnHellModeToggle);
            ApplyHellMode();
        }

        void OnRules()  => Debug.Log("[TitleTopBar] ルール表示（未実装）");
        void OnTopics() => Debug.Log("[TitleTopBar] お題選択（未実装）");

        void OnHellModeToggle()
        {
            IsHellMode = !IsHellMode;
            ApplyHellMode();
        }

        void ApplyHellMode()
        {
            if (hellButtonBg)
                hellButtonBg.color = IsHellMode ? BtnLime : BtnLemon;
            if (backgroundImage)
                backgroundImage.color = IsHellMode ? BgHell : BgNormal;
            if (lemonRain)
                lemonRain.SetTexture(IsHellMode ? limeTexture : lemonTexture);
            if (startButtonImage && startNormalTexture && startLimeTexture)
                startButtonImage.texture = IsHellMode ? startLimeTexture : startNormalTexture;
        }
    }
}
