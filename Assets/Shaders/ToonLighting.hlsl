#ifdef SHADERGRAPH_PREVIEW
#else
half3 GetAdditionalLightColor(float3 Normal, float3 WPos, out half Intensity)
{
	Light addLight = GetAdditionalLight(0, WPos, 1);
	half lightDot = dot(Normal, addLight.direction);
	//Reduce shadow strength
	lightDot =lightDot*0.6+ 0.4;
	Intensity = smoothstep(0, 0.02, lightDot* addLight.distanceAttenuation) * addLight.shadowAttenuation * saturate(GetAdditionalLightsCount());

	return lerp(0,addLight.color,Intensity);
}
#endif

half GetSmoothnessPower(half smoothness)
{
	return exp2(10 * smoothness +1);
}

sampler2D _LightCookie;
#define COOKIE_TILING 0.05
void ToonShading_float(in float ToonRampSmoothness,in float ToonRampOffset, in float3 Normal, in float3 ClipSpacePos, in float3 WorldPos, in float3 ViewDir, in float Glossiness, out float3 ToonRampOutput, out float3 LightDirection)
{

	// set the shader graph node previews
#ifdef SHADERGRAPH_PREVIEW
	ToonRampOutput = float3(0.5, 0.5, 0);
	LightDirection = half3(0,0,0);
#else

	// grab the shadow coordinates
#if SHADOWS_SCREEN
	half4 shadowCoord = ComputeScreenPos(ClipSpacePos);
#else
	half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif 

	// grab the main light
#if _MAIN_LIGHT_SHADOWS_CASCADE || _MAIN_LIGHT_SHADOWS
	Light light = GetMainLight(shadowCoord);
#else
	Light light = GetMainLight();
#endif

//Additional Lights
	half addLightIntensity = 0;
	float3 addLightCol = float3(0,0,0);
#ifdef _ADDITIONAL_LIGHTS
	addLightCol = GetAdditionalLightColor(Normal, WorldPos, addLightIntensity);
#endif

	//Cookie
	half cookie = tex2D(_LightCookie, WorldPos.xz * COOKIE_TILING).r;

	/* //Light
	half NdotL = dot(Normal, light.direction);
	half lightIntensity = smoothstep(0, 0.1, NdotL * light.shadowAttenuation) * cookie;
	half3 lightTint = lerp(half4(0,0,0.2,1), light.color, saturate(lightIntensity));

	//Specular
	//Half vector is the vetwor in between view dir and light dir
	float3 halfVector = normalize(ViewDir+light.direction);
	float NdotH = dot(Normal, halfVector);
	//Change size based on glossiness
	float specularIntensity = pow(NdotH * lightIntensity, Glossiness *400);
	//Change intensity based on glossiness
	specularIntensity = max(smoothstep(0.005, 0.03, specularIntensity), addLightIntensity) * Glossiness; */
	
	// dot product for toonramp
	half d = dot(Normal, light.direction) * 0.5 + 0.5;
	
	// toonramp in a smoothstep
	half toonRamp = smoothstep(ToonRampOffset, ToonRampOffset+ ToonRampSmoothness, d );
	toonRamp *= light.shadowAttenuation;
	
	//Specular
	half specularDot = saturate(dot(Normal, normalize(light.direction + ViewDir)));
	//half specular = pow(specularDot, GetSmoothnessPower(Glossiness))*d;
	float specularThreshold = 0.85 + Glossiness * 0.12;
	half specular2 = smoothstep(specularThreshold, specularThreshold+ToonRampSmoothness*0.1, specularDot) *Glossiness *2 * light.shadowAttenuation;
	
	ToonRampOutput = light.color * (toonRamp+specular2) + addLightCol;
	// output direction for rimlight
	LightDirection = light.direction;
#endif
}