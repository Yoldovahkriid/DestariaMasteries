#version 330 core

in vec2 texCoord;
out vec4 outColor;

uniform sampler2D screenTexture;
uniform vec2 uResolution;

uniform float fadeAlpha;    // 0 = fully revealing the game, 1 = fully black
uniform float gearRotation; // radians, accumulates continuously (negative = CCW)
uniform float glowPulse;    // 0..1 breathing pulse
uniform float iTime;
uniform vec3 gearTint;

// Signed "coverage" mask (0..1) for a simple toothed gear centered at the origin.
float gearMask(vec2 p, float baseRadius, float toothDepth, float toothCount)
{
    float angle = atan(p.y, p.x);
    float radius = length(p);

    float toothWave = cos(angle * toothCount);
    float toothProfile = smoothstep(0.15, 0.55, toothWave);
    float outerRadius = baseRadius + toothDepth * toothProfile;

    float outerEdge = smoothstep(outerRadius, outerRadius - 0.006, radius);
    float innerEdge = smoothstep(baseRadius * 0.42, baseRadius * 0.42 + 0.006, radius);
    float ring = outerEdge * innerEdge;

    // Hub hole in the middle of the gear.
    float hub = smoothstep(baseRadius * 0.16, baseRadius * 0.16 - 0.006, radius);

    return clamp(ring - hub, 0.0, 1.0);
}

void main()
{
    vec3 sceneColor = texture(screenTexture, texCoord).rgb;
    vec3 blackedOut = mix(sceneColor, vec3(0.0), fadeAlpha);

    // Aspect-correct, screen-centered coordinates so the gear stays circular on any resolution.
    vec2 centered = (texCoord - 0.5) * vec2(uResolution.x / uResolution.y, 1.0);

    float c = cos(gearRotation);
    float s = sin(gearRotation);
    vec2 rotated = vec2(c * centered.x - s * centered.y, s * centered.x + c * centered.y);

    float mask = gearMask(rotated, 0.17, 0.035, 10.0);

    vec3 gearColor = gearTint * (0.75 + 0.5 * glowPulse);
    float softGlow = mask * (0.4 + 0.6 * glowPulse) * 0.2;

    // The gear only shows through once the black fade has (mostly) covered the scene, and it
    // fades with the same alpha so it never pops in against unfaded gameplay.
    vec3 finalColor = mix(blackedOut, gearColor, mask * fadeAlpha) + gearTint * softGlow * fadeAlpha;

    outColor = vec4(finalColor, 1.0);
}
