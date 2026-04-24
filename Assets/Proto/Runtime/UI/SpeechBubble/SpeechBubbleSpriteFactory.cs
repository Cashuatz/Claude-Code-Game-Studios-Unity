using System.Collections.Generic;
using UnityEngine;

namespace Proto.UI.Speech
{
    /// <summary>
    /// 말풍선용 프로시저 스프라이트 팩토리.
    ///  - 9-slice 용 라운디드 사각형 (코너 반경 파라미터화)
    ///  - 말풍선 꼬리 (아래를 가리키는 이등변삼각형, pivot = (0.5, 1))
    /// 동일 스펙은 static 캐시로 1회만 생성.
    /// 알파 커버리지 기반 AA (경계 0.5 블렌딩).
    /// </summary>
    public static class SpeechBubbleSpriteFactory
    {
        private static readonly Dictionary<(int size, int radius), Sprite> s_roundCache = new();
        private static readonly Dictionary<(int w, int h), Sprite> s_triCache = new();

        /// <summary>9-slice 라운디드 사각형 스프라이트. border = radius + 1.</summary>
        public static Sprite GetRoundedRect(int size, int radius)
        {
            size = Mathf.Max(8, size);
            radius = Mathf.Clamp(radius, 0, size / 2 - 1);
            var key = (size, radius);
            if (s_roundCache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = BuildRoundedRectTexture(size, radius);
            var rect = new Rect(0f, 0f, size, size);
            var pivot = new Vector2(0.5f, 0.5f);
            var border = Vector4.one * (radius + 1);
            var sprite = Sprite.Create(tex, rect, pivot, 100f, 0, SpriteMeshType.FullRect, border);
            sprite.name = $"SB_Rounded_{size}_{radius}";
            s_roundCache[key] = sprite;
            return sprite;
        }

        /// <summary>아래쪽(v=0) 이 꼭지점, 위쪽(v=1) 이 가장 넓은 이등변삼각형. pivot = (0.5, 1).</summary>
        public static Sprite GetTriangle(int width, int height)
        {
            width = Mathf.Max(4, width);
            height = Mathf.Max(4, height);
            var key = (width, height);
            if (s_triCache.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = BuildTriangleTexture(width, height);
            var rect = new Rect(0f, 0f, width, height);
            var pivot = new Vector2(0.5f, 1f);
            var sprite = Sprite.Create(tex, rect, pivot, 100f);
            sprite.name = $"SB_Triangle_{width}_{height}";
            s_triCache[key] = sprite;
            return sprite;
        }

        private static Texture2D BuildRoundedRectTexture(int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"T_SB_Rounded_{size}_{radius}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            var px = new Color[size * size];
            float r = radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 코너 중심(=사각형 안쪽 radius 만큼 들어간 박스 경계) 로 픽셀 중심을 클램프
                    float px_ = x + 0.5f;
                    float py_ = y + 0.5f;
                    float cx = Mathf.Clamp(px_, r, size - r);
                    float cy = Mathf.Clamp(py_, r, size - r);
                    float dx = px_ - cx;
                    float dy = py_ - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // r - dist 가 양수면 안쪽. 경계에서 0.5 폭 블렌딩으로 AA.
                    float a = (r <= 0f) ? 1f : Mathf.Clamp01(r - dist + 0.5f);
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(px);
            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D BuildTriangleTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                name = $"T_SB_Triangle_{w}_{h}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            var pixels = new Color[w * h];
            float halfW = w * 0.5f;
            float H = Mathf.Max(1f, h - 1);

            for (int y = 0; y < h; y++)
            {
                // y = 0 (최하단) → 폭 0 (꼭지점), y = h-1 (최상단) → 폭 w 전체
                float t = y / H;
                float halfWidthAtY = halfW * t;
                for (int x = 0; x < w; x++)
                {
                    float dx = (x + 0.5f) - halfW;
                    float coverage = Mathf.Clamp01(halfWidthAtY - Mathf.Abs(dx) + 0.5f);
                    pixels[y * w + x] = new Color(1f, 1f, 1f, coverage);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return tex;
        }
    }
}
