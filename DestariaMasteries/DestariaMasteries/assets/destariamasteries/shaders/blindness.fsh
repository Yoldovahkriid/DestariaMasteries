#version 330 core

uniform float intensity;
uniform float iTime;
uniform vec2 uResolution;
uniform sampler2D screenTexture;

out vec4 outColor;

void main() 
{
    vec2 uv = gl_FragCoord.xy / uResolution.xy;
    
    // Calculate heartbeat pulse (varies between 0.8 and 1.2)
    float pulse = 1.0 + (sin(iTime * 1.0) * 0.2 * intensity);
    
    // Distance from center for vignette and blur scaling
    float dist = length(uv - 0.5);
    
    // --- BLUR LOGIC ---
    // The blur strength increases as we move away from the center
    // and increases with the 'intensity' and 'pulse'
    float blurAmount = dist * 0.08 * intensity * pulse;
    vec4 blurredColor = vec4(0.0);
    
    // Basic 9-sample blur (Gaussian-lite)
    blurredColor += texture(screenTexture, uv + vec2(-blurAmount, -blurAmount));
    blurredColor += texture(screenTexture, uv + vec2(0, -blurAmount));
    blurredColor += texture(screenTexture, uv + vec2(blurAmount, -blurAmount));
    blurredColor += texture(screenTexture, uv + vec2(-blurAmount, 0));
    blurredColor += texture(screenTexture, uv + vec2(0, 0));
    blurredColor += texture(screenTexture, uv + vec2(blurAmount, 0));
    blurredColor += texture(screenTexture, uv + vec2(-blurAmount, blurAmount));
    blurredColor += texture(screenTexture, uv + vec2(0, blurAmount));
    blurredColor += texture(screenTexture, uv + vec2(blurAmount, blurAmount));
    blurredColor /= 9.0;

    // --- VIGNETTE / BLACKOUT LOGIC ---
    // Create a dark ring that gets tighter with the pulse
    float vignette = smoothstep(0.1, 0.3 / pulse, dist);
    
    // Convert blurred color to grayscale
    float gray = dot(blurredColor.rgb, vec3(0.299, 0.587, 0.114));
    vec3 grayColor = vec3(gray);

    // Darken the edges, but keep the blurred center slightly visible
    vec3 finalRGB = mix(grayColor, vec3(0.0), vignette * intensity);
    
    // Apply a final total blackout based on intensity
    float finalAlpha = clamp(intensity, 0.0, 1.0);
    
    outColor = vec4(finalRGB, finalAlpha);
}