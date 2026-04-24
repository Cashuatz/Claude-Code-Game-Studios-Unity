using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Proto.Rendering
{
    public sealed class FullscreenSampleFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
            [Range(0f, 1f)] public float effectStrength   = 0.85f;
            [Range(0f, 1f)] public float vignetteRadius   = 0.80f;
            [Range(0f, 1f)] public float vignetteSoftness = 0.45f;
            [Range(0f, 1f)] public float scanlineStrength = 0.12f;
        }

        public Settings settings = new Settings();

        private Material _material;
        private FullscreenSamplePass _pass;

        private static readonly int ID_EffectStrength   = Shader.PropertyToID("_EffectStrength");
        private static readonly int ID_VignetteRadius   = Shader.PropertyToID("_VignetteRadius");
        private static readonly int ID_VignetteSoftness = Shader.PropertyToID("_VignetteSoftness");
        private static readonly int ID_ScanlineStrength = Shader.PropertyToID("_ScanlineStrength");
        private static readonly int ID_BlitTexture      = Shader.PropertyToID("_BlitTexture");

        public override void Create()
        {
            var shader = Shader.Find("Proto/Rendering/FullscreenSample");
            if (shader == null)
            {
                Debug.LogError("[FullscreenSampleFeature] Shader 'Proto/Rendering/FullscreenSample' not found.");
                return;
            }
            _material = CoreUtils.CreateEngineMaterial(shader);
            _pass = new FullscreenSamplePass(_material, ID_BlitTexture)
            {
                renderPassEvent = settings.injectionPoint,
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null || _material == null) return;
            if (renderingData.cameraData.cameraType != CameraType.Game &&
                renderingData.cameraData.cameraType != CameraType.SceneView) return;

            _material.SetFloat(ID_EffectStrength,   settings.effectStrength);
            _material.SetFloat(ID_VignetteRadius,   settings.vignetteRadius);
            _material.SetFloat(ID_VignetteSoftness, settings.vignetteSoftness);
            _material.SetFloat(ID_ScanlineStrength, settings.scanlineStrength);

            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_material);
            _material = null;
        }

        private sealed class FullscreenSamplePass : ScriptableRenderPass
        {
            private readonly Material _material;
            private readonly int _blitTextureId;

            public FullscreenSamplePass(Material material, int blitTextureId)
            {
                _material = material;
                _blitTextureId = blitTextureId;
                profilingSampler = new ProfilingSampler("Proto.FullscreenSample");
                requiresIntermediateTexture = true;
            }

            private class CopyData
            {
                public TextureHandle source;
            }

            private class DrawData
            {
                public TextureHandle source;
                public Material material;
                public Mesh mesh;
                public int blitTextureId;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                var cameraData   = frameData.Get<UniversalCameraData>();

                if (resourceData.isActiveTargetBackBuffer) return;

                TextureHandle cameraColor = resourceData.activeColorTexture;

                var desc = cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                TextureHandle tempColor = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph, desc, "_ProtoFullscreenSampleTemp", false);

                // Pass 1: CameraColor → tempColor
                using (var builder = renderGraph.AddRasterRenderPass<CopyData>(
                    "Proto.FullscreenSample.Copy", out var copyData, profilingSampler))
                {
                    copyData.source = cameraColor;
                    builder.UseTexture(cameraColor, AccessFlags.Read);
                    builder.SetRenderAttachment(tempColor, 0, AccessFlags.Write);
                    builder.SetRenderFunc((CopyData d, RasterGraphContext ctx) =>
                    {
                        Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1, 1, 0, 0), 0, false);
                    });
                }

                // Pass 2: tempColor → CameraColor (fullscreen triangle mesh + custom material)
                using (var builder = renderGraph.AddRasterRenderPass<DrawData>(
                    "Proto.FullscreenSample.Draw", out var drawData, profilingSampler))
                {
                    drawData.source = tempColor;
                    drawData.material = _material;
                    drawData.mesh = FullscreenTriangle.Get();
                    drawData.blitTextureId = _blitTextureId;

                    builder.UseTexture(tempColor, AccessFlags.Read);
                    builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);
                    builder.AllowGlobalStateModification(true);
                    builder.SetRenderFunc((DrawData d, RasterGraphContext ctx) =>
                    {
                        ctx.cmd.SetGlobalTexture(d.blitTextureId, d.source);
                        ctx.cmd.DrawMesh(d.mesh, Matrix4x4.identity, d.material, 0, 0);
                    });
                }
            }
        }
    }
}