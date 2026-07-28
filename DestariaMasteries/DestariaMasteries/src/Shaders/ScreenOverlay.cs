using Vintagestory.API.Client;

namespace DestariaMasteries.src.Shaders
{
    // Used in ScreenEffectRenderer to render arbitary amounts of shaders
    public abstract class ScreenOverlayEffect
    {
        public abstract string Id { get; }

        public IShaderProgram ShaderProgram { get; set; }

        public virtual bool IsFinished => false;

        public abstract void Update(float dt);

        public abstract void Render(ICoreClientAPI capi, MeshRef quadRef);

        public virtual void Dispose()
        {
        }
    }
}