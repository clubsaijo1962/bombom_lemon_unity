using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BomBomLemon.Title
{
    public class LemonRainEffect : MonoBehaviour
    {
        [SerializeField] private Texture2D lemonTexture;
        [SerializeField] private int particleCount = 18;
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
            public int column;
        }

        readonly List<Particle> _particles = new();

        void Start()
        {
            // 全パーティクルを画面上方から開始し、画面表示時に自然に降り始める
            float colWidth = canvasWidth / particleCount;
            float topBase  = canvasHeight * 0.5f + maxSize;

            for (int i = 0; i < particleCount; i++)
            {
                float x = -canvasWidth * 0.5f + colWidth * i
                          + Random.Range(colWidth * 0.1f, colWidth * 0.9f);
                // 列ごとに縦方向をずらして一斉に現れないようにする
                float y = topBase + (canvasHeight / particleCount) * i
                          + Random.Range(0f, canvasHeight * 0.08f);
                CreateParticle(x, y, i);
            }
        }

        void CreateParticle(float startX, float startY, int column)
        {
            var go = new GameObject("LemonDrop", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0.5f, 0.5f);
            rect.anchorMax        = new Vector2(0.5f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            float size            = Random.Range(minSize, maxSize);
            rect.sizeDelta        = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(startX, startY);
            rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));

            if (lemonTexture != null)
            {
                var raw           = go.AddComponent<RawImage>();
                raw.texture       = lemonTexture;
                raw.raycastTarget = false;
                raw.color         = new Color(1f, 1f, 1f, Random.Range(0.28f, 0.55f));
            }

            _particles.Add(new Particle
            {
                rect     = rect,
                speed    = Random.Range(minSpeed, maxSpeed),
                rotSpeed = Random.Range(-55f, 55f),
                column   = column,
            });
        }

        public void SetTexture(Texture2D tex)
        {
            lemonTexture = tex;
            foreach (var p in _particles)
            {
                if (!p.rect) continue;
                var raw = p.rect.GetComponent<RawImage>();
                if (raw) raw.texture = tex;
            }
        }

        void Update()
        {
            float dt       = Time.deltaTime;
            float bottom   = -canvasHeight * 0.5f - maxSize;
            float top      =  canvasHeight * 0.5f + maxSize;
            float colWidth = canvasWidth / particleCount;

            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                if (!p.rect) continue;

                var pos = p.rect.anchoredPosition;
                pos.y -= p.speed * dt;
                p.rect.anchoredPosition = pos;
                p.rect.localEulerAngles += new Vector3(0f, 0f, p.rotSpeed * dt);

                if (pos.y < bottom)
                {
                    // 担当列の範囲内でX位置をランダムに決めて上から再登場
                    float sectionLeft = -canvasWidth * 0.5f + colWidth * p.column;
                    float x = sectionLeft + Random.Range(colWidth * 0.05f, colWidth * 0.95f);
                    p.rect.anchoredPosition = new Vector2(x, top);
                    p.rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
                }
            }
        }
    }
}
