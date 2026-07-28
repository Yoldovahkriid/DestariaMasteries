using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DestariaMasteries.src.Shaders
{
    // Used to render an arbitary amount of shaders at the same time
    public class ScreenEffectsRenderer : IRenderer
    {
        readonly ICoreClientAPI capi;
        readonly MeshRef quadRef;
        readonly List<ScreenOverlayEffect> effects = new List<ScreenOverlayEffect>();

        public ScreenEffectsRenderer(ICoreClientAPI capi)
        {
            this.capi = capi;

            MeshData quadMesh = QuadMeshUtil.GetCustomQuadModelData(-1, -1, 0, 2, 2);
            quadMesh.Rgba = null;

            quadRef = capi.Render.UploadMesh(quadMesh);
        }

        public double RenderOrder => 0.25;

        public int RenderRange => 1;

        public void AddEffect(ScreenOverlayEffect effect)
        {
            effects.Add(effect);
        }

        public bool HasEffect(string id)
        {
            foreach (var effect in effects)
            {
                if (effect.Id == id) return true;
            }
            return false;
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (effects.Count == 0) return;

            IShaderProgram currShader = capi.Render.CurrentActiveShader;
            currShader?.Stop();

            int screenTexId = capi.Render.FrameBuffers[(int)EnumFrameBuffer.Primary].ColorTextureIds[0];
            Vec2f resolution = new Vec2f(capi.Render.FrameWidth, capi.Render.FrameHeight);

            capi.Render.GlToggleBlend(true);

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                ScreenOverlayEffect effect = effects[i];
                effect.Update(deltaTime);

                if (effect.IsFinished)
                {
                    effect.Dispose();
                    effects.RemoveAt(i);
                    continue;
                }

                if (effect.ShaderProgram == null) continue;

                capi.Render.BindTexture2d(screenTexId);
                effect.ShaderProgram.Use();
                effect.ShaderProgram.Uniform("screenTexture", 0);
                effect.ShaderProgram.Uniform("uResolution", resolution);

                effect.Render(capi, quadRef);

                effect.ShaderProgram.Stop();
            }

            currShader?.Use();
        }

        public void Dispose()
        {
            foreach (ScreenOverlayEffect effect in effects)
            {
                effect.Dispose();
            }
            effects.Clear();

            capi.Render.DeleteMesh(quadRef);
        }
    }
}