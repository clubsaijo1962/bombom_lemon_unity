using UnityEngine;
using UnityEngine.UI;

namespace BomBomLemon.Title
{
    public class TitleTopBarController : MonoBehaviour
    {
        [SerializeField] private Button rulesButton;
        [SerializeField] private Button topicsButton;
        [SerializeField] private Button hellModeButton;
        [SerializeField] private Image hellModeIndicator;

        public bool IsHellMode { get; private set; } = false;

        static readonly Color IndicatorOff = new Color(0.55f, 0.55f, 0.55f);
        static readonly Color IndicatorOn  = new Color(0.95f, 0.35f, 0.20f);

        void Start()
        {
            rulesButton?.onClick.AddListener(OnRules);
            topicsButton?.onClick.AddListener(OnTopics);
            hellModeButton?.onClick.AddListener(OnHellModeToggle);
            UpdateHellIndicator();
        }

        void OnRules()
        {
            Debug.Log("[TitleTopBar] ルール表示（未実装）");
        }

        void OnTopics()
        {
            Debug.Log("[TitleTopBar] お題選択（未実装）");
        }

        void OnHellModeToggle()
        {
            IsHellMode = !IsHellMode;
            UpdateHellIndicator();
            Debug.Log($"[TitleTopBar] 地獄モード: {(IsHellMode ? "ON" : "OFF")}");
        }

        void UpdateHellIndicator()
        {
            if (hellModeIndicator)
                hellModeIndicator.color = IsHellMode ? IndicatorOn : IndicatorOff;
        }
    }
}
