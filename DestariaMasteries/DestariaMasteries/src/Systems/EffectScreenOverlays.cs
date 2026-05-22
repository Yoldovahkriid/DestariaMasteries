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
        IShaderProgram OverlayShaderProg;
        BlindnessShader renderer;

        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;

            api.Event.ReloadShader += LoadShader;
            LoadShader();

            renderer = new BlindnessShader(capi, OverlayShaderProg);
            api.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
        }

        public bool LoadShader()
        {
            OverlayShaderProg = capi.Shader.NewShaderProgram();

            OverlayShaderProg.AssetDomain = Mod.Info.ModID;

            capi.Shader.RegisterFileShaderProgram("blindness", OverlayShaderProg);
            OverlayShaderProg.Compile();

            if (renderer != null)
            {
                renderer.shaderProgram = OverlayShaderProg;
            }

            return true;
        }
    }
}
