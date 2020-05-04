struct PS_IN
{
    float4 Position : SV_POSITION;
    float3 WorldPosition : POSITION;
    float3 Normal : NORMAL;
    float4 Color : COLOR;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float2 UV2 : TEXCOORD2;
    float2 UV3 : TEXCOORD3;
};

struct PS_OUT
{
    float4 Color : SV_TARGET;
};

Texture2D<float4> DiffuseTexture : register(t0);
SamplerState TextureSampler : register(s0);

PS_OUT main(PS_IN input)
{
    PS_OUT result;


    float4 albedo = input.Color;
    albedo.xyz *= DiffuseTexture.Sample(TextureSampler, input.UV0);

    // Lambertian shading
    if (length(input.Normal) > 0)
        result.Color = float4(albedo.rgb * saturate(dot(input.Normal, float3(0, 1, 0)) * 0.5f + 0.5f), albedo.a);
    else
        result.Color = albedo;

    return result;
}