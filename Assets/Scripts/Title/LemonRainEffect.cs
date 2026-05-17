using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BomBomLemon.Title
{
    public class LemonRainEffect : MonoBehaviour
    {
        [SerializeField] private Texture2D lemonTexture;
        [SerializeField] private int particleCount = 16;
        [SerializeField] private float minSize = 122f;
        [SerializeField] private float maxSize = 244f;
        [SerializeField] private float minSpeed = 110f;
        [SerializeField] private float maxSpeed = 260f;
        [SerializeField] private float canvasWidth = 1080f;
        [SerializeField] private float canvasHeight = 1920f;

        struct Particle
        {
            public RectTransform rect;
            public float speed;
            public float rotSpeed;
        }

        readonly List<Particle> _particles = new();

        void Start()
        {
            // 横方向を均等分割し、縦も均等にずらして配置
            int cols = particleCount;
            float colWidth = canvasWidth / cols;
            for (int i = 0; i < cols; i++)
            {
                float x = -canvasWidth * 0.5f + colWidth * i + Random.Range(colWidth * 0.1f, colWidth * 0.9f);
                float y = Mathf.Lerp(-canvasHeight * 0.5f, canvasHeight * 0.5f, (float)i / cols)
                          + Random.Range(-canvasHeight * 0.05f, canvasHeight * 0.05f);
                CreateParticle(x, Mathf.Clamp(y, -canvasHeight * 0.5f, canvasHeight * 0.5f));
            }
        }

        void CreateParticle(float startX, float startY)
        {
            var go = new GameObject("LemonDrop", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);

            float size = Random.Range(minSize, maxSize);
            rect.sizeDelta        = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(startX, startY);
            rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));

            if (lemonTexture != null)
            {
                var raw = go.AddComponent<RawImage>();
                raw.texture       = lemonTexture;
                raw.raycastTarget = false;
                raw.color         = new Color(1f, 1f, 1f, Random.Range(0.30f, 0.60f));
            }

            _particles.Add(new Particle
            {
                rect     = rect,
                speed    = Random.Range(minSpeed, maxSpeed),
                rotSpeed = Random.Range(-55f, 55f),
            });
        }

        void Update()
        {
            float dt     = Time.deltaTime;
            float bottom = -canvasHeight * 0.5f - maxSize;
            float top    =  canvasHeight * 0.5f + maxSize;
            float colWidth = canvasWidth / particleCount;

            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                if (!p.rect) continue;

                var pos = p.rect.anchoredPosition;
                pos.y -= p.speed * dt;
                p.rect.anchoredPosition = pos;
                p.rect.localEulerAngles += new Vector3(0f, 0f, p.rotSpeed * dt);

                // 画面下から出たら、担当列の範囲内でランダムX・上から再登場
                if (pos.y < bottom)
                {
                    float section = i * colWidth;
                    float x = -canvasWidth * 0.5f + section + Random.Range(colWidth * 0.05f, colWidth * 0.95f);
                    p.rect.anchoredPosition = new Vector2(x, top);
                    p.rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
                }
            }
        }
    }
}
