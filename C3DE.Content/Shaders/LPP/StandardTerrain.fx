#include "../Forward/ShadowMap.fxh"
#include "../Forward/Lights.fxh"
#include "Lights.fxh"

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 DiffuseColor = float3(1.0, 1.0, 1.0);

// Misc
float2 TextureTiling = float2(1, 1);
float3 EyePosition = float3(1, 1, 0);

texture2D MainTexture;
sampler2D MainTextureSampler = sampler_state
{
    Texture = <MainTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D SandTexture;
sampler2D SandTextureSampler = sampler_state
{
    Texture = <SandTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D RockTexture;
sampler2D RockTextureSampler = sampler_state
{
    Texture = <RockTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D SnowTexture;
sampler2D SnowTextureSampler = sampler_state
{
    Texture = <SnowTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D WeightMap;
sampler2D WeightMapSampler = sampler_state
{
    Texture = <WeightMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
#if SM4
	float4 Position : SV_Position;
#else
    float4 Position : POSITION0;
#endif
    float2 UV : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
    float4 CopyPosition : POSITION1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, World);
    output.WorldPosition = worldPosition;
    output.CopyPosition = output.Position;
    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 sandTex = tex2D(SandTextureSampler, input.UV * TextureTiling);
    float3 rockTex = tex2D(RockTextureSampler, input.UV * TextureTiling);
    float3 snowTex = tex2D(SnowTextureSampler, input.UV * TextureTiling);
    float3 mainTex = tex2D(MainTextureSampler, input.UV * TextureTiling);
    float3 weightTex = tex2D(WeightMapSampler, input.UV);
	
    float3 diffuse = clamp(1.0 - weightTex.r - weightTex.g - weightTex.b, 0, 1);
    diffuse *= mainTex;
    diffuse += weightTex.r * sandTex + weightTex.g * rockTex + weightTex.b * snowTex;
	
    diffuse = diffuse * DiffuseColor;

    float3 light = GetLightingValue(input.CopyPosition);
    float shadow = CalcShadow(input.WorldPosition);
    float3 diffuse2 = light * shadow * diffuse;
    float3 specular = CalcSpecular(input.WorldPosition, input.WorldNormal, EyePosition, input.UV * TextureTiling);
    float3 compose = diffuse2 + specular;

    return float4(compose, 1);
}

technique Terrain
{
    pass AmbientPass
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }
}