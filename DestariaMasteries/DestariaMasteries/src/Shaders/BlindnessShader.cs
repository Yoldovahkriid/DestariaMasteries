using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DestariaMasteries.src.Shaders
{
    public class BlindnessShader : IRenderer
    {
        MeshRef quadRef;
        ICoreClientAPI capi;
        public IShaderProgram shaderProgram;

        float intensity = 0f;
        float fadeSpeed = 1.5f;
        float totalTime = 0f;

        public BlindnessShader(ICoreClientAPI capi, IShaderProgram shaderProgram)
        {
            this.capi = capi;
            this.shaderProgram = shaderProgram;

            MeshData quadMesh = QuadMeshUtil.GetCustomQuadModelData(-1, -1, 0, 2, 2);
            quadMesh.Rgba = null;

            quadRef = capi.Render.UploadMesh(quadMesh);
        }
        public double RenderOrder => 0.25;

        public int RenderRange => 1;

        public void Dispose()
        {
            capi.Render.DeleteMesh(quadRef);
            shaderProgram.Dispose();
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            bool isBlinded = capi.World.Player.Entity.WatchedAttributes.GetAsBool("IsBlinded", false);

            if (isBlinded) intensity = Math.Min(1f, intensity + deltaTime * fadeSpeed);
            else intensity = Math.Max(0f, intensity - deltaTime * fadeSpeed);

            if (intensity <= 0) return;

            totalTime += deltaTime;

            IShaderProgram currShader = capi.Render.CurrentActiveShader;
            currShader.Stop();

            shaderProgram.Use();

            int screenTexId = capi.Render.FrameBuffers[(int)EnumFrameBuffer.Primary].ColorTextureIds[0];
            capi.Render.BindTexture2d(screenTexId);

            shaderProgram.Uniform("intensity", intensity);
            shaderProgram.Uniform("iTime", totalTime);
            shaderProgram.Uniform("uResolution", new Vec2f(capi.Render.FrameWidth, capi.Render.FrameHeight));
            shaderProgram.Uniform("screenTexture", 0);

            capi.Render.GlToggleBlend(true);
            capi.Render.RenderMesh(quadRef);
            shaderProgram.Stop();

            currShader.Use();
        }
    }
}
