#include "StandardBase.fx"

// Material
float3 DiffuseColor;
float2 TextureTiling = float2(1, 1);
float3 EmissiveColor;
float3 SpecularColor;
int EmissiveEnabled;

texture AlbedoMap;
sampler albedoSampler = sampler_state
{
    Texture = (AlbedoMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture EmissiveMap;
sampler emissiveSampler = sampler_state
{
    Texture = (EmissiveMap);
    MAGFILTER = LINEAR;
    MINFILTER = LINEAR;
    MIPFILTER = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 MainPS(VertexShaderOutput input) : COLOR0
{ 
	float2 scaledUV = input.UV * TextureTiling;

	// Albedo
	float4 albedo = tex2D(albedoSampler, scaledUV);
	
	// Normal
	float3 normal = input.WorldNormal;
	
	// Emissive
	float3 emissive = float3(0, 0, 0);
    if (EmissiveEnabled > 0)
		emissive = tex2D(emissiveSampler, scaledUV).xyz * EmissiveColor;

	// Base Pixel Shader
	return float4(StandardPixelShader(input.WorldPosition, normal, SpecularColor, albedo.rgb * DiffuseColor, emissive), albedo.a);
}

technique Standard
{
    pass P0
    {
#if SM4
		VertexShader = compile vs_4_0 MainVS();
		PixelShader = compile ps_4_0 MainPS();
#else
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
#endif
    }
}

technique Standard_Instanced
{
	pass P0
	{
#if SM4
		VertexShader = compile vs_4_0 MainVS_Instanced();
		PixelShader = compile ps_4_0 MainPS();
#else
		VertexShader = compile vs_3_0 MainVS_Instanced();
		PixelShader = compile ps_3_0 MainPS();
#endif
	}
}