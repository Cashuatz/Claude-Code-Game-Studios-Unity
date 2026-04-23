using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto.Shared.ShaderFx
{
    /// <summary>
    /// Proto 공용 셰이더 FX 엔트리포인트.
    /// - 머티리얼은 static 싱글톤 캐시에서 공유 (Shader.Find → new Material).
    /// - 렌더러 개별 파라미터는 MaterialPropertyBlock (MPB) 로만 제어.
    /// - 트윈은 자체 Coroutine, Time.unscaledDeltaTime 사용 (Time.timeScale 무영향).
    /// </summary>
    public static class ShaderFxHub
    {
        // 셰이더 프로퍼티 ID 캐시
        public static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");
        public static readonly int DissolveNoise  = Shader.PropertyToID("_DissolveNoise");
        public static readonly int EdgeColor      = Shader.PropertyToID("_EdgeColor");
        public static readonly int EdgeWidth      = Shader.PropertyToID("_EdgeWidth");
        public static readonly int RimColor       = Shader.PropertyToID("_RimColor");
        public static readonly int RimPower       = Shader.PropertyToID("_RimPower");
        public static readonly int RimIntensity   = Shader.PropertyToID("_RimIntensity");
        public static readonly int HoloColor      = Shader.PropertyToID("_HoloColor");
        public static readonly int ScanlineColor  = Shader.PropertyToID("_ScanlineColor");
        public static readonly int ScanlineSpeed  = Shader.PropertyToID("_ScanlineSpeed");
        public static readonly int ScanlineDensity= Shader.PropertyToID("_ScanlineDensity");
        public static readonly int Glitch         = Shader.PropertyToID("_Glitch");
        public static readonly int Alpha          = Shader.PropertyToID("_Alpha");
        public static readonly int FlashColor     = Shader.PropertyToID("_FlashColor");
        public static readonly int FlashAmount    = Shader.PropertyToID("_FlashAmount");
        public static readonly int OutlineColor   = Shader.PropertyToID("_OutlineColor");
        public static readonly int OutlineWidth   = Shader.PropertyToID("_OutlineWidth");
        public static readonly int BaseColor      = Shader.PropertyToID("_BaseColor");

        private static readonly Dictionary<ShaderFxRecipe, Material> _mats = new();
        private static Texture2D _noiseTex;
        private static MonoBehaviour _host;
        private static readonly MaterialPropertyBlock _mpb = new();

        private static void EnsureHost()
        {
            if (_host != null) return;
            var go = new GameObject("[ShaderFxHub]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _host = go.AddComponent<ShaderFxHubRunner>();
            go.hideFlags = HideFlags.HideInHierarchy;
        }

        private static Texture2D GetNoiseTex()
        {
            if (_noiseTex != null) return _noiseTex;
            const int N = 128;
            var tex = new Texture2D(N, N, TextureFormat.R8, false, true) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            var px = new Color32[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float v = Mathf.PerlinNoise(x * 0.07f, y * 0.07f) * 0.6f
                        + Mathf.PerlinNoise(x * 0.21f, y * 0.21f) * 0.3f
                        + Mathf.PerlinNoise(x * 0.53f, y * 0.53f) * 0.1f;
                byte b = (byte)(Mathf.Clamp01(v) * 255f);
                px[y * N + x] = new Color32(b, b, b, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            _noiseTex = tex;
            return _noiseTex;
        }

        public static Material GetSharedMaterial(ShaderFxRecipe recipe)
        {
            if (_mats.TryGetValue(recipe, out var m) && m != null) return m;

            var shaderName = recipe switch
            {
                ShaderFxRecipe.DissolveIn       => "Proto/Fx/Dissolve",
                ShaderFxRecipe.DissolveOut      => "Proto/Fx/Dissolve",
                ShaderFxRecipe.DissolveEdgeGlow => "Proto/Fx/Dissolve",
                ShaderFxRecipe.RimLight         => "Proto/Fx/RimLight",
                ShaderFxRecipe.Hologram         => "Proto/Fx/Hologram",
                ShaderFxRecipe.Scanline         => "Proto/Fx/Scanline",
                ShaderFxRecipe.HitFlash         => "Proto/Fx/HitFlash",
                ShaderFxRecipe.Outline          => "Proto/Fx/Outline",
                _ => null,
            };
            if (shaderName == null) return null;

            var sh = Shader.Find(shaderName);
            if (sh == null)
            {
                Debug.LogError($"[ShaderFxHub] Shader not found: {shaderName}");
                return null;
            }

            var mat = new Material(sh) { name = $"M_Fx_{recipe}" };
            ApplyRecipeDefaults(mat, recipe);
            _mats[recipe] = mat;
            return mat;
        }

        private static void ApplyRecipeDefaults(Material mat, ShaderFxRecipe recipe)
        {
            switch (recipe)
            {
                case ShaderFxRecipe.DissolveIn:
                case ShaderFxRecipe.DissolveOut:
                    mat.SetTexture(DissolveNoise, GetNoiseTex());
                    mat.SetColor(EdgeColor, new Color(1f, 0.5f, 0.1f, 1f));
                    mat.SetFloat(EdgeWidth, 0.05f);
                    mat.SetColor(BaseColor, new Color(0.85f, 0.85f, 0.85f, 1f));
                    break;
                case ShaderFxRecipe.DissolveEdgeGlow:
                    mat.SetTexture(DissolveNoise, GetNoiseTex());
                    mat.SetColor(EdgeColor, new Color(0.3f, 0.9f, 1.2f, 1f));
                    mat.SetFloat(EdgeWidth, 0.12f);
                    mat.SetColor(BaseColor, new Color(0.85f, 0.85f, 0.85f, 1f));
                    break;
                case ShaderFxRecipe.RimLight:
                    mat.SetColor(BaseColor, new Color(0.35f, 0.4f, 0.5f, 1f));
                    mat.SetColor(RimColor, new Color(0.3f, 1.2f, 1.6f, 1f));
                    mat.SetFloat(RimPower, 2.2f);
                    mat.SetFloat(RimIntensity, 0.5f);
                    break;
                case ShaderFxRecipe.Hologram:
                    mat.SetColor(HoloColor, new Color(0.3f, 0.9f, 1.2f, 1f));
                    mat.SetFloat(Alpha, 0.75f);
                    mat.SetFloat(ScanlineSpeed, 2f);
                    mat.SetFloat(ScanlineDensity, 25f);
                    break;
                case ShaderFxRecipe.Scanline:
                    mat.SetColor(BaseColor, new Color(0.1f, 0.1f, 0.15f, 1f));
                    mat.SetColor(ScanlineColor, new Color(0.2f, 1f, 0.5f, 1f));
                    mat.SetFloat(ScanlineSpeed, 6f);
                    mat.SetFloat(ScanlineDensity, 80f);
                    mat.SetFloat(Glitch, 0.25f);
                    break;
                case ShaderFxRecipe.HitFlash:
                    mat.SetColor(BaseColor, new Color(0.7f, 0.2f, 0.2f, 1f));
                    mat.SetColor(FlashColor, new Color(1f, 1f, 1f, 1f));
                    mat.SetFloat(FlashAmount, 0f);
                    break;
                case ShaderFxRecipe.Outline:
                    mat.SetColor(BaseColor, new Color(0.8f, 0.4f, 0.2f, 1f));
                    mat.SetColor(OutlineColor, new Color(0f, 0f, 0f, 1f));
                    mat.SetFloat(OutlineWidth, 0.03f);
                    break;
            }
        }

        /// <summary>레시피 적용: 렌더러에 sharedMaterial 할당 + 트윈 시작.</summary>
        public static Coroutine Apply(
            Renderer target,
            ShaderFxRecipe recipe,
            float durationSec = 0.8f,
            Action onComplete = null)
        {
            if (target == null) return null;
            EnsureHost();

            var mat = GetSharedMaterial(recipe);
            if (mat == null) return null;
            target.sharedMaterial = mat;

            return _host.StartCoroutine(TweenRoutine(target, recipe, durationSec, onComplete));
        }

        public static Coroutine ApplyGroup(
            IReadOnlyList<Renderer> targets,
            ShaderFxRecipe recipe,
            float durationSec = 0.8f,
            Action onComplete = null)
        {
            if (targets == null || targets.Count == 0) return null;
            EnsureHost();

            var mat = GetSharedMaterial(recipe);
            if (mat == null) return null;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null) targets[i].sharedMaterial = mat;
            }
            return _host.StartCoroutine(TweenGroupRoutine(targets, recipe, durationSec, onComplete));
        }

        /// <summary>수동 제어: 진행도(0~1) 즉시 세팅.</summary>
        public static void SetProgress(Renderer target, ShaderFxRecipe recipe, float progress01)
        {
            if (target == null) return;
            var mat = GetSharedMaterial(recipe);
            if (mat == null) return;
            if (target.sharedMaterial != mat) target.sharedMaterial = mat;

            target.GetPropertyBlock(_mpb);
            WriteProgress(_mpb, recipe, Mathf.Clamp01(progress01));
            target.SetPropertyBlock(_mpb);
        }

        private static void WriteProgress(MaterialPropertyBlock mpb, ShaderFxRecipe recipe, float t)
        {
            switch (recipe)
            {
                case ShaderFxRecipe.DissolveIn:
                    // 등장: amount 1 → 0
                    mpb.SetFloat(DissolveAmount, 1f - t);
                    break;
                case ShaderFxRecipe.DissolveOut:
                    // 퇴장: amount 0 → 1
                    mpb.SetFloat(DissolveAmount, t);
                    break;
                case ShaderFxRecipe.DissolveEdgeGlow:
                    // 엣지 강조 퇴장
                    mpb.SetFloat(DissolveAmount, t);
                    mpb.SetFloat(EdgeWidth, Mathf.Lerp(0.05f, 0.2f, t));
                    break;
                case ShaderFxRecipe.RimLight:
                    mpb.SetFloat(RimIntensity, Mathf.Lerp(0.3f, 2.4f, t));
                    break;
                case ShaderFxRecipe.Hologram:
                    mpb.SetFloat(Alpha, Mathf.Lerp(0.35f, 1.1f, t));
                    break;
                case ShaderFxRecipe.Scanline:
                    mpb.SetFloat(Glitch, Mathf.Lerp(0f, 0.6f, t));
                    break;
                case ShaderFxRecipe.HitFlash:
                    // 0 → 1 → 0 (ping-pong)
                    float pingPong = t < 0.5f ? t * 2f : (1f - t) * 2f;
                    mpb.SetFloat(FlashAmount, pingPong);
                    break;
                case ShaderFxRecipe.Outline:
                    mpb.SetFloat(OutlineWidth, Mathf.Lerp(0.005f, 0.045f, t));
                    break;
            }
        }

        private static IEnumerator TweenRoutine(Renderer target, ShaderFxRecipe recipe, float dur, Action onComplete)
        {
            if (target == null) yield break;
            float elapsed = 0f;
            while (elapsed < dur)
            {
                if (target == null) yield break;
                float t = Easings.EaseOutQuad(elapsed / Mathf.Max(0.0001f, dur));
                target.GetPropertyBlock(_mpb);
                WriteProgress(_mpb, recipe, t);
                target.SetPropertyBlock(_mpb);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            if (target != null)
            {
                target.GetPropertyBlock(_mpb);
                WriteProgress(_mpb, recipe, 1f);
                target.SetPropertyBlock(_mpb);
            }
            onComplete?.Invoke();
        }

        private static IEnumerator TweenGroupRoutine(IReadOnlyList<Renderer> targets, ShaderFxRecipe recipe, float dur, Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < dur)
            {
                float t = Easings.EaseOutQuad(elapsed / Mathf.Max(0.0001f, dur));
                for (int i = 0; i < targets.Count; i++)
                {
                    var r = targets[i];
                    if (r == null) continue;
                    r.GetPropertyBlock(_mpb);
                    WriteProgress(_mpb, recipe, t);
                    r.SetPropertyBlock(_mpb);
                }
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            onComplete?.Invoke();
        }

        // 외부 인스턴스 없이 Coroutine 을 구동하기 위한 숨김 러너.
        private sealed class ShaderFxHubRunner : MonoBehaviour { }
    }
}
