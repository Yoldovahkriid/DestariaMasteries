using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DestariaMasteries.src.Shaders
{
    public class TurnBackTheClockOverlayEffect : ScreenOverlayEffect
    {
        public override string Id => "turnbacktheclock";

        const float FadeInDuration = 0.6f;
        const float HoldDuration = 4.4f;
        const float FadeOutDuration = 0.8f;
        const float TotalDuration = FadeInDuration + HoldDuration + FadeOutDuration;

        // Negative = counter-clockwise.
        const float GearRotationSpeedRad = -0.6f;

        float elapsed = 0f;
        float gearRotation = 0f;

        public TurnBackTheClockOverlayEffect(IShaderProgram shaderProgram)
        {
            ShaderProgram = shaderProgram;
        }

        public override bool IsFinished => elapsed >= TotalDuration;

        public override void Update(float dt)
        {
            elapsed += dt;
            gearRotation += GearRotationSpeedRad * dt;
        }

        public override void Render(ICoreClientAPI capi, MeshRef quadRef)
        {
            float fadeAlpha = ComputeFadeAlpha();

            // Gentle pulse so the gear doesn't look static while the screen is held dark.
            float glowPulse = 0.5f + 0.5f * (float)System.Math.Sin(elapsed * 2.2f);

            ShaderProgram.Uniform("fadeAlpha", fadeAlpha);
            ShaderProgram.Uniform("gearRotation", gearRotation);
            ShaderProgram.Uniform("glowPulse", glowPulse);
            ShaderProgram.Uniform("iTime", elapsed);
            ShaderProgram.Uniform("gearTint", new Vec3f(0.15f, 0.95f, 0.95f));

            capi.Render.RenderMesh(quadRef);
        }

        float ComputeFadeAlpha()
        {
            if (elapsed < FadeInDuration)
            {
                return GameMath.Clamp(elapsed / FadeInDuration, 0f, 1f);
            }

            if (elapsed < FadeInDuration + HoldDuration)
            {
                return 1f;
            }

            float fadeOutElapsed = elapsed - FadeInDuration - HoldDuration;
            return 1f - GameMath.Clamp(fadeOutElapsed / FadeOutDuration, 0f, 1f);
        }
    }
}