#ifndef QUICK_MIRROR_REFLECTION_INC
#define QUICK_MIRROR_REFLECTION_INC

/////////////////////////////////////////////////////////
// UNIFORM PARAMETERS 
/////////////////////////////////////////////////////////
uniform sampler2D _LeftEyeTexture;			//The texture containing the reflection of the DEFAULT geometry for the left eye
uniform sampler2D _RightEyeTexture;			//The texture containing the reflection of the DEFAULT geometry for the right eye
uniform sampler2D _NoiseMask;				//A texture used to create imperfections in the reflection
uniform float _ReflectionPower;				//Indicates how much light is the reflection. It is used to simulate the light lost during the reflection
uniform float _NoisePower;					//Indicates how much powerful is the noise texture
uniform float4 _NoiseColor;					//The color of the noise

uniform int REFLECTION_INVERT_Y;

struct appdata
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;

	UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
};

struct v2f
{
	float3 uv : TEXCOORD0;
	float4 screenPos : TEXCOORD1;
	UNITY_FOG_COORDS(2)
	float4 pos : SV_POSITION;

	UNITY_VERTEX_OUTPUT_STEREO //Insert
};

float2 GetProjUV(float4 screenPos) 
{
	float2 projUV = screenPos.xy / screenPos.w;

#if UNITY_SINGLE_PASS_STEREO
	float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
	projUV = (projUV - scaleOffset.zw) / scaleOffset.xy;
#endif

	return projUV;
}

v2f vert(appdata v)
{
	v2f o;

	UNITY_SETUP_INSTANCE_ID(v); //Insert
	UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert

	o.pos = UnityObjectToClipPos(v.pos);
	o.uv.xy = v.uv;
	o.uv.z = unity_StereoEyeIndex;
	o.screenPos = ComputeScreenPos(o.pos);

	return o;
}

fixed4 ComputeFinalColor(float2 uvReflection, float3 uvTex) 
{
	//return isEyeLeft() ? fixed4(1,0,0,1) : fixed4(0,1,0,1);

	fixed4 refl = uvTex.z == 0 ? tex2D(_LeftEyeTexture, uvReflection) : tex2D(_RightEyeTexture, uvReflection);
	//fixed4 refl = uvTex.z == 0 ? fixed4(1, 0, 0, 1) : fixed4(0, 1, 0, 1);
	fixed4 noiseColor = tex2D(_NoiseMask, uvTex) * _NoiseColor;
	fixed4 finalColor = refl * _ReflectionPower + noiseColor * _NoisePower;

	return saturate(finalColor);
}

fixed4 ComputeFinalColor_MOBILE(float2 uvReflection, float3 uvTex)
{
	//return isEyeLeft() ? fixed4(1,0,0,1) : fixed4(0,1,0,1);

	return uvTex.z == 0 ? tex2D(_LeftEyeTexture, uvReflection) : tex2D(_RightEyeTexture, uvReflection);
}

#endif