Shader "EffectShader"
{
	/*
     * Some details on what we tried to achieve with the bonus shader:
     * Making an "Invisible Ink" effect - Texture that can only be seen when there is no light.
     * To make it even more nice looking, the hidden texture also flickers using Perlin noise.
     * Hopefully, this gives the intended effect - magical text that has moving energy inside of it.
     * Loosely inspired by The Doors of Durin - the entrance to Moria from the LOTR books.
     * 
     * To do this we took "basic" shader - based on our brick shader, and added this extra functionality to it.
     * Making this basically an "Additive", something you can probably add to most shaders.
     * The bonus scene uses RoundedCube and Capsule to show the effect.
     */
    Properties
    {
        _NormalColor ("Color", Color) = (0, 0, 0, 1)
        [NoScaleOffset] _HiddenMap ("HiddenMap Map", 2D) = "defaulttexture" {}
        _HiddenColor ("Hidden Color", Color) = (0.8, 0.85, 1, 1)
        _TimeScale("Time Scale", Range(0.1, 5)) = 1
        _NoiseScale("Noise Scale", Range(1, 100)) = 35

    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Declare used properties
            uniform sampler2D _HiddenMap;
            uniform fixed4 _HiddenColor;
            uniform float _NoiseScale;
            uniform float _TimeScale;
            uniform fixed4 _NormalColor;


            // Returns a psuedo-random float3 with componenets between -1 and 1 for a given float3 c 
            float3 random3(float3 c)
            {
                float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
                float3 r;
                r.z = frac(512.0 * j);
                j *= .125;
                r.x = frac(512.0 * j);
                j *= .125;
                r.y = frac(512.0 * j);
                r = -1.0 + 2.0 * r;
                return r.yzx;
            }

            // Interpolates a given array v of 8 float3 values using triquintic interpolation
            // at the given ratio t (a float3 with components between 0 and 1)
            float triquinticInterpolation(float3 v[8], float3 t)
            {
                // Your implementation
                float3 u = (6.0 * t * t - 15.0 * t + 10.0) * t * t * t; // Quintic interpolation

                // Interpolate in the x direction
                const float x1 = lerp(v[0], v[1], u.x);
                const float x2 = lerp(v[2], v[3], u.x);

                const float x3 = lerp(v[4], v[5], u.x);
                const float x4 = lerp(v[6], v[7], u.x);

                // Interpolate in the y direction
                const float y1 = lerp(x1, x2, u.y);
                const float y2 = lerp(x3, x4, u.y);

                // Interpolate in the z direction and return
                return lerp(y1, y2, u.z);
            }

            // Returns the value of a 3D Perlin noise function at the given coordinates c
            float perlin3d(float3 c)
            {
                // Your implementation
                const float3 c_int0 = float3(floor(c.x), floor(c.y), floor(c.z));
                const float3 c_int1 = c_int0 + float3(1, 0, 0);
                const float3 c_int2 = c_int0 + float3(0, 1, 0);
                const float3 c_int3 = c_int0 + float3(1, 1, 0);
                const float3 c_int4 = c_int0 + float3(0, 0, 1);
                const float3 c_int5 = c_int0 + float3(1, 0, 1);
                const float3 c_int6 = c_int0 + float3(0, 1, 1);
                const float3 c_int7 = c_int0 + float3(1, 1, 1);

                float3 dot_product[8] = {
                    float3(dot(c - c_int0, random3(c_int0)), 0, 0),
                    float3(dot(c - c_int1, random3(c_int1)), 0, 0),
                    float3(dot(c - c_int2, random3(c_int2)), 0, 0),
                    float3(dot(c - c_int3, random3(c_int3)), 0, 0),
                    float3(dot(c - c_int4, random3(c_int4)), 0, 0),
                    float3(dot(c - c_int5, random3(c_int5)), 0, 0),
                    float3(dot(c - c_int6, random3(c_int6)), 0, 0),
                    float3(dot(c - c_int7, random3(c_int7)), 0, 0)
                };

                return triquinticInterpolation(dot_product, c - c_int0);
            }

            float waterNoise(float2 uv, float t)
            {
                // Your implementation
                return perlin3d(0.5 * float3(uv, t))
                    + 0.5 * perlin3d(float3(uv, t))
                    + 0.2 * perlin3d(float3(2.0 * uv, 3.0 * t));
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float noise = waterNoise(input.uv * _NoiseScale, _TimeScale * _Time.y);
                noise = noise * 0.5 + 0.5;
                const float3 hidden = noise * _HiddenColor * (1 - tex2D(_HiddenMap, input.uv));
                return fixed4(_NormalColor + hidden, 1.0);
            }
            ENDCG
        }
    }
}