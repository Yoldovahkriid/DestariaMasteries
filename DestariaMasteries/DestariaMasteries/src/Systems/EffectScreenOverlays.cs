using DestariaMasteries.src.Shaders;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace DestariaMasteries.src.Systems
{
    internal class EffectScreenOverlays : ModSystem
    {
        ICoreClientAPI capi;

        ScreenEffectsRenderer effectsRenderer;
        IShaderProgram blindnessShaderProg;
        IShaderProgram turnBackTheClockShaderProg;
        BlindnessOverlayEffect blindnessEffect;

        const string TurnBackTheClockTriggerKey = "destariamasteries:turnbacktheclock-trigger";
        long lastSeenTurnBackTheClockTrigger = 0L;

        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;

            api.Event.ReloadShader += LoadShaders;
            LoadShaders();

            effectsRenderer = new ScreenEffectsRenderer(capi);
            api.Event.RegisterRenderer(effectsRenderer, EnumRenderStage.Ortho);

            blindnessEffect = new BlindnessOverlayEffect(capi, blindnessShaderProg);
            effectsRenderer.AddEffect(blindnessEffect);

            api.Event.RegisterGameTickListener(CheckTurnBackTheClockTrigger, 100);
        }

        void CheckTurnBackTheClockTrigger(float dt)
        {
            Vintagestory.API.Common.Entities.Entity player = capi.World.Player?.Entity;
            if (player == null) return;

            long triggerValue = player.WatchedAttributes.GetLong(TurnBackTheClockTriggerKey, 0L);
            if (triggerValue != 0L && triggerValue != lastSeenTurnBackTheClockTrigger)
            {
                lastSeenTurnBackTheClockTrigger = triggerValue;
                PlayTurnBackTheClockOverlay();
            }
        }

        public void PlayTurnBackTheClockOverlay()
        {
            if (effectsRenderer == null || turnBackTheClockShaderProg == null) return;
            if (effectsRenderer.HasEffect("turnbacktheclock")) return;

            effectsRenderer.AddEffect(new TurnBackTheClockOverlayEffect(turnBackTheClockShaderProg));
        }

        public bool LoadShaders()
        {
            blindnessShaderProg?.Dispose();
            turnBackTheClockShaderProg?.Dispose();

            blindnessShaderProg = capi.Shader.NewShaderProgram();
            blindnessShaderProg.AssetDomain = Mod.Info.ModID;
            capi.Shader.RegisterFileShaderProgram("blindness", blindnessShaderProg);
            blindnessShaderProg.Compile();

            turnBackTheClockShaderProg = capi.Shader.NewShaderProgram();
            turnBackTheClockShaderProg.AssetDomain = Mod.Info.ModID;
            capi.Shader.RegisterFileShaderProgram("turnbacktheclock", turnBackTheClockShaderProg);
            turnBackTheClockShaderProg.Compile();

            if (blindnessEffect != null)
            {
                blindnessEffect.ShaderProgram = blindnessShaderProg;
            }

            return true;
        }

        public override void Dispose()
        {
            blindnessShaderProg?.Dispose();
            turnBackTheClockShaderProg?.Dispose();
            effectsRenderer?.Dispose();

            base.Dispose();
        }
    }
}