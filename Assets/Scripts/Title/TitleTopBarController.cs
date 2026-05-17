using UnityEngine;
using UnityEngine.UI;

namespace BomBomLemon.Title
{
    public class TitleTopBarController : MonoBehaviour
    {
        [SerializeField] private Button rulesButton;
        [SerializeField] private Button topicsButton;
        [SerializeField] private Button hellModeButton;
        [SerializeField] private Image hellToggleTrack;
        [SerializeField] private RectTransform hellToggleKnob;

        public bool IsHellMode { get; private set; } = false;

        static readonly Color TrackOff = new Color(0.50f, 0.50f, 0.52f, 1f);
        static readonly Color TrackOn  = new Color(0.95f, 0.32f, 0.12f, 1f);

        void Start()
        {
            rulesButton?.onClick.AddListener(OnRules);
            topicsButton?.onClick.AddListener(OnTopics);
            hellModeButton?.onClick.AddListener(OnHellModeToggle);
            UpdateHellIndicator();
        }

        void OnRules()  => Debug.Log("[TitleTopBar] ルール表示（未実装）");
        void OnTopics() => Debug.Log("[TitleTopBar] お題選択（未実装）");

        void OnHellModeToggle()
        {
            IsHellMode = !IsHellMode;
            UpdateHellIndicator();
        }

        void UpdateHellIndicator()
        {
            if (hellToggleTrack)
                hellToggleTrack.color = IsHellMode ? TrackOn : TrackOff;
            if (hellToggleKnob)
                hellToggleKnob.anchoredPosition = new Vector2(IsHellMode ? 12f : -12f, 0f);
        }
    }
}
