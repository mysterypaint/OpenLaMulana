#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler s0;
extern float time;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float3 Hsv2rgb(float3 input)
{
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(input.xxx + K.xyz) * 6.0 - K.www);

	return input.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), input.y);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 texColor = tex2D(s0, input.TextureCoordinates);
    float4 transpWhite = float4(1, 1, 1, 0);
	
	//float4 color = ScreenTexture.Sample(TextureSampler, texCoord.xy);
    /*
	if (all(texColor == transpWhite)) {
        //texColor = _Color1out;
		clip(-1);
    }*/
	
    texColor = all(texColor == transpWhite) ? -1 : texColor;
	
    clip(texColor);
	//color.gb = color.r;
	
    texColor.rgb *= Hsv2rgb(float3(time, 1, 1));
    return texColor;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};