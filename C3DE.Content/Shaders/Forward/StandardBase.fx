#include "../Common/Macros.fxh"
#include "../Common/Fog.fxh"

// Constants
#if SM4
#define MAX_LIGHT_COUNT 128
#else
#define MAX_LIGHT_COUNT 8
#endif

// Lighting
// LightData.x: Type: Directional, Point, Spot
// LightData.y: Intensity
// LightData.z: Range
// LightData.w: FallOff
// SpotData.xyz: Direction
// SpotData.w: Angle

float3 LightPosition[MAX_LIGHT_COUNT];
float3 LightColor[MAX_LIGHT_COUNT];
float4 LightData[MAX_LIGHT_COUNT];
float4 SpotData[MAX_LIGHT_COUNT];
int LightCount = 0;

#if REFLECTION_MAP
float4x4 ReflectionView;
#endif

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Material
float3 AmbientColor;
int SpecularPower;

// Misc
float3 EyePosition;

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
#if REFLECTION_MAP
	float4 Reflection : TEXCOORD3;
#endif
    float3x3 WorldToTangentSpace : TEXCOORD4;
    float FogDistance : FOG;
};

struct VSOutput_VL
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
    float3 Color : TEXCOORD3;
	#if REFLECTION_MAP
	float4 Reflection : TEXCOORD4;
#endif
	float FogDistance : FOG;
};

// ---
// --- Pixel Lighting Vertex Shader
// ---
VertexShaderOutput CommonVS(VertexShaderInput input, float4x4 instanceTransform)
{
	VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.UV = input.UV;
    output.WorldNormal = mul(input.Normal, instanceTransform);
    output.WorldPosition = worldPosition;
    output.FogDistance = distance(worldPosition.xyz, EyePosition);

    float3 c1 = cross(input.Normal, float3(0.0, 0.0, 1.0));
    float3 c2 = cross(input.Normal, float3(0.0, 1.0, 0.0));

    // [0] Tangent / [1] Binormal / [2] Normal
    output.WorldToTangentSpace[0] = length(c1) > length(c2) ? c1 : c2;
    output.WorldToTangentSpace[1] = normalize(output.WorldToTangentSpace[0]);
    output.WorldToTangentSpace[2] = input.Normal;

#if REFLECTION_MAP
	float4x4 preReflectionViewProjection = mul(ReflectionView, Projection);
	float4x4 preWorldReflectionViewProjection = mul(instanceTransform, preReflectionViewProjection);
	output.Reflection = mul(input.Position, preWorldReflectionViewProjection);
#endif

    return output;
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	return CommonVS(input, World);
}

VertexShaderOutput MainVS_Instancing(VertexShaderInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
	return CommonVS(input, mul(World, transpose(instanceTransform)));
}

// ---
// --- Lighting Calculation for directional, point and spot.
// ---
float3 CalculateOneLight(int i, float3 worldPosition, float3 worldNormal, float3 diffuseColor, float3 specularColor)
{
	float3 lightVector = LightPosition[i] - worldPosition;
	float3 directionToLight = normalize(lightVector);
	float diffuseIntensity = saturate(dot(directionToLight, worldNormal));
	
	if (diffuseIntensity <= 0)
		return float3(0, 0, 0);
	
	float3 diffuse = diffuseIntensity * LightColor[i] * diffuseColor;
	float baseIntensity = 1; // Directional
	
	if (LightData[i].x == 1) // Point
	{	
		float d = length(lightVector);
		baseIntensity = 1.0 - pow(saturate(d / LightData[i].z), LightData[i].w);
	}
	else if (LightData[i].x == 2) // Spot
	{
		float d = dot(directionToLight, normalize(SpotData[i].xyz));
		float a = cos(SpotData[i].w);
		
		if (a < d)
			baseIntensity = 1.0 - pow(clamp(a / d, 0.0, 1.0), LightData[i].w);
		else
			baseIntensity = 0.0;
	}
	
	// Self Shadow.
	float selfShadow = saturate(4 * diffuseIntensity);
	
	// Phong
	float3 reflectionVector = normalize(reflect(-directionToLight, worldNormal));
	float3 directionToCamera = normalize(EyePosition - worldPosition);
	float3 specular = saturate(LightColor[i] * specularColor * pow(saturate(dot(reflectionVector, directionToCamera)), SpecularPower));
			   
	return  selfShadow * baseIntensity * (diffuse + specular);
}

// Standard Pixel Shader for Per Pixel Lighting
float3 StandardPixelShader(float4 worldPosition, float3 normal, float3 specularTerm, float fogDistance, float3 albedo, float3 emissive, float shadowTerm, float4 reflection)
{   
	float3 light;
	int limit = LightCount;

#if !SM4
	limit = min(MAX_LIGHT_COUNT, LightCount);
#endif

	for (int i = 0; i < limit; i++)
	{
		light += CalculateOneLight(i, worldPosition.xyz, normal, albedo, specularTerm);
	}

	if (reflection.a > 0)
		albedo = lerp(albedo, reflection.xyz, reflection.a);

    float3 color = AmbientColor + (albedo * light * shadowTerm) + emissive;
	color = ApplyFog(color, fogDistance);

	return color;
}

// Standard Pixel Shader for Vertex Lighting
float3 StandardPixelShader_VL(float3 light, float fogDistance, float3 albedo, float3 emissive, float shadowTerm, float4 reflection)
{   
	if (reflection.a > 0)
		albedo = lerp(albedo, reflection.xyz, reflection.a);

    float3 color = AmbientColor + (albedo * light * shadowTerm) + emissive;
	color = ApplyFog(color, fogDistance);

	return color;
}