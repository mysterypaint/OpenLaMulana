#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler TextureSampler : register(s0);

Texture2D PrevViewTexture;
sampler2D PrevViewSampler = sampler_state
{
    Texture = <PrevViewTexture>;
};

Texture2D MaskTexture;
sampler2D MaskSampler = sampler_state
{
    Texture = <MaskTexture>;
};

//float4 MainPS(VertexShaderOutput input) : COLOR
float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0,
float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 curr = tex2D(TextureSampler, texCoord);
    float4 prevView = tex2D(PrevViewSampler, texCoord);
    float4 mask = tex2D(MaskSampler, texCoord);
    float4 transp = round(80.0f / 255.0f);
    float4 solidGray = float4(0.5, 0.5, 0.5, 1);
    //float4 texColor = lerp(mask, curr, 0.0001f);
    
    bool isEqual = !any(mask.a - solidGray.a);
    float4 texColor = isEqual ? prevView : curr;
    clip(texColor);
    
    return texColor;
}

technique SpriteDrawing
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}