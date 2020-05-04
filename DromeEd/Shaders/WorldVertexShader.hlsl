//#pragma pack_matrix(row_major)

struct VS_IN
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float4 Color : COLOR;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float2 UV2 : TEXCOORD2;
    float2 UV3 : TEXCOORD3;
};

struct VS_OUT
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

cbuffer FrameConstants : register(b0)
{
    float4x4 Projection;
    float4x4 View;
}

cbuffer ObjectConstants : register(b1)
{
    float4x4 Model;
}

VS_OUT main(VS_IN input)
{
    VS_OUT result;

    result.WorldPosition = mul(Model, float4(input.Position, 1.0f)).xyz;
    //result.Position = mul(mul(float4(result.WorldPosition, 1.0f), View), Projection);
    result.Position = mul(Projection, mul(View, float4(result.WorldPosition, 1.0f)));
    result.Normal = mul(float4(input.Normal, 0.0f), Model).xyz;
    result.Color = input.Color;
    result.UV0 = input.UV0;
    result.UV1 = input.UV1;
    result.UV2 = input.UV2;
    result.UV3 = input.UV3;

    return result;
}