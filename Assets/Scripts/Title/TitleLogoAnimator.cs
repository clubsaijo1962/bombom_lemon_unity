using UnityEngine;

namespace BomBomLemon.Title
{
    public class TitleLogoAnimator : MonoBehaviour
    {
        [Header("Bubble Layer - ゆっくり大きく揺れる")]
        [SerializeField] private RectTransform layerBubble;
        [SerializeField] private float bubbleFloatSpeed = 0.55f;
        [SerializeField] private float bubbleFloatY = 8f;
        [SerializeField] private float bubbleScaleSpeed = 0.45f;
        [SerializeField] private float bubbleScaleAmp = 0.018f;

        [Header("Lemon Layer - ぷにぷに＋微回転")]
        [SerializeField] private RectTransform layerLemon;
        [SerializeField] private float lemonFloatSpeed = 0.85f;
        [SerializeField] private float lemonFloatY = 12f;
        [SerializeField] private float lemonRotSpeed = 0.5f;
        [SerializeField] private float lemonRotAmp = 2.5f;
        [SerializeField] private float lemonScaleSpeed = 1.1f;
        [SerializeField] private float lemonScaleAmp = 0.035f;

        [Header("Word Layer - 独立した位相で揺れる")]
        [SerializeField] private RectTransform layerWord;
        [SerializeField] private float wordFloatSpeed = 0.7f;
        [SerializeField] private float wordFloatY = 9f;
        [SerializeField] private float wordScaleSpeed = 0.9f;
        [SerializeField] private float wordScaleAmp = 0.022f;

        [Header("Start Button - 呼吸するようにスケール")]
        [SerializeField] private RectTransform startButton;
        [SerializeField] private float startScaleSpeed = 1.4f;
        [SerializeField] private float startScaleAmp = 0.07f;

        Vector2 _bubbleBase, _lemonBase, _wordBase, _startBase;

        void Start()
        {
            if (layerBubble) _bubbleBase = layerBubble.anchoredPosition;
            if (layerLemon)  _lemonBase  = layerLemon.anchoredPosition;
            if (layerWord)   _wordBase   = layerWord.anchoredPosition;
            if (startButton) _startBase  = startButton.anchoredPosition;
        }

        void Update()
        {
            float t = Time.time;

            if (layerBubble)
            {
                float y = Mathf.Sin(t * bubbleFloatSpeed) * bubbleFloatY;
                float x = Mathf.Sin(t * bubbleFloatSpeed * 0.6f + 0.5f) * bubbleFloatY * 0.25f;
                layerBubble.anchoredPosition = _bubbleBase + new Vector2(x, y);
                float s = 1f + Mathf.Sin(t * bubbleScaleSpeed + 0.3f) * bubbleScaleAmp;
                layerBubble.localScale = Vector3.one * s;
            }

            if (layerLemon)
            {
                // 泡とは違う位相でふわふわ
                float y = Mathf.Sin(t * lemonFloatSpeed + 1.1f) * lemonFloatY;
                layerLemon.anchoredPosition = _lemonBase + new Vector2(0f, y);
                float rot = Mathf.Sin(t * lemonRotSpeed + 0.5f) * lemonRotAmp;
                layerLemon.localEulerAngles = new Vector3(0f, 0f, rot);
                // ぷにぷにスケール
                float s = 1f + Mathf.Sin(t * lemonScaleSpeed + 1.5f) * lemonScaleAmp;
                layerLemon.localScale = Vector3.one * s;
            }

            if (layerWord)
            {
                // レモンとは逆位相で揺れる
                float y = Mathf.Sin(t * wordFloatSpeed + 2.3f) * wordFloatY;
                layerWord.anchoredPosition = _wordBase + new Vector2(0f, y);
                float s = 1f + Mathf.Sin(t * wordScaleSpeed + 0.9f) * wordScaleAmp;
                layerWord.localScale = Vector3.one * s;
            }

            if (startButton)
            {
                // 呼吸するようなスケールパルス
                float s = 1f + Mathf.Sin(t * startScaleSpeed) * startScaleAmp;
                startButton.localScale = Vector3.one * s;
            }
        }
    }
}
