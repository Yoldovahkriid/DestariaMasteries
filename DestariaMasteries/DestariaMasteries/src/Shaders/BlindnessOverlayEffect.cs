using Vintagestory.API.Client;

namespace DestariaMasteries.src.Shaders
{
    public class BlindnessOverlayEffect : ScreenOverlayEffect
    {
        readonly ICoreClientAPI capi;

        float intensity = 0f;
        readonly float fadeSpeed = 1.5f;
        float totalTime = 0f;

        public override string Id => "blindness";

        public BlindnessOverlayEffect(ICoreClientAPI capi, IShaderProgram shaderProgram)
        {
            this.capi = capi;
            ShaderProgram = shaderProgram;
        }

        public override void Update(float dt)
        {
            bool isBlinded = capi.World.Player.Entity.WatchedAttributes.GetAsBool("IsBlinded", false);

            if (isBlinded) intensity = System.Math.Min(1f, intensity + dt * fadeSpeed);
            else intensity = System.Math.Max(0f, intensity - dt * fadeSpeed);

            totalTime += dt;
        }

        public override void Render(ICoreClientAPI capi, MeshRef quadRef)
        {
            // Nothing to draw while fully faded out should be cheaper than issuing a zero-alpha draw call.
            if (intensity <= 0f) return;

            ShaderProgram.Uniform("intensity", intensity);
            ShaderProgram.Uniform("iTime", totalTime);

            capi.Render.RenderMesh(quadRef);
        }
    }
}