shader_type canvas_item;

uniform sampler2D textureMask;
uniform sampler2D textureWave;

uniform sampler2D textureNoise : repeat_enable;

uniform vec4 color;

const float c_textureHeight = 448.0f;
const float c_textureWidth = 2760.0f;
const float c_heightToWidth = c_textureHeight / c_textureWidth;

const float c_textureMaskDepth = 24.0f;

const float c_noiseCutOffHeight = 0.985f;
const float c_noiseCutOffWidth = 0.925f;
const float c_noiseMaxAmplitude = 28.0f;

const float c_noiseAWaveCount = 16.0f * TAU;
const float c_noiseAWaveSpeedX = -0.02f;
const float c_noiseAWaveSpeedY = 0.002f;

const float c_noiseBWaveCount = 24.0f * TAU;
const float c_noiseBWaveSpeedX = 0.025f;
const float c_noiseBWaveSpeedY = -0.003f;

const float c_noiseCWaveCount = 12.0f * TAU;
const float c_noiseCWaveSpeedX = -0.03f;
const float c_noiseCWaveSpeedY = 0.0025f;

const float c_noiseWaveHeightOffset = c_textureMaskDepth / c_textureHeight;
const float c_noiseWaveDepthOffset = c_textureMaskDepth / c_textureWidth;

const float c_noiseColorSpeed = 0.02f;

float Remap(
	float value,
	float fromMin,
	float fromMax,
	float toMin,
	float toMax
)
{
    return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
}

vec3 SampleNoiseAHorizontal(
	vec2 uv
)
{
	float x = uv.x;
	float y = uv.y;
	
	float wave = c_noiseMaxAmplitude * sin(
		x * c_noiseAWaveCount - TIME
	);
	
	vec2 offset = vec2(
		TIME * c_noiseAWaveSpeedX,
		TIME * c_noiseAWaveSpeedY
	);
	float colorNoise = texture(
		textureNoise,
		uv + offset
	).r * wave;
	
	float colorRemap = 	Remap(
		colorNoise,
		-1.0f,
		1.0f,
		c_noiseCutOffWidth,
		1.0f
	);
	
	float colorFinal = step(
		colorRemap,
		y + c_noiseWaveHeightOffset
	);
	
	return vec3(
		colorFinal,
		colorFinal,
		colorFinal
	);
}

vec3 SampleNoiseAVertical(
	vec2 uv
)
{
	const float waveCount = c_noiseAWaveCount * c_heightToWidth;
	
	float x = uv.x;
	float y = uv.y;
	
	float wave =  c_noiseMaxAmplitude * sin(
		y * waveCount - TIME
	);
	
	vec2 offset = vec2(
		TIME * c_noiseAWaveSpeedY,
		TIME * c_noiseAWaveSpeedX
	);
	float colorNoise = texture(
		textureNoise,
		uv + offset
	).r * wave;
	
	float colorRemap = 	Remap(
		colorNoise,
		-1.0f,
		1.0f,
		c_noiseCutOffHeight,
		1.0f
	);
	
	float colorFinal = step(
		colorRemap,
		x + c_noiseWaveDepthOffset
	);
	
	return vec3(
		colorFinal,
		colorFinal,
		colorFinal
	);
}

vec3 SampleNoiseBHorizontal(
	vec2 uv
)
{
	float x = uv.x;
	float y = uv.y;
	
	float wave =  c_noiseMaxAmplitude * sin(
		x * c_noiseBWaveCount - TIME
	);
	
	vec2 offset = vec2(
		TIME * c_noiseBWaveSpeedX,
		TIME * c_noiseBWaveSpeedY
	);
	float colorNoise = texture(
		textureNoise,
		uv + offset
	).r * wave;
	
	float colorRemap = 	Remap(
		colorNoise,
		-1.0f,
		1.0f,
		c_noiseCutOffWidth,
		1.0f
	);
	
	float colorFinal = step(
		colorRemap,
		y + c_noiseWaveHeightOffset
	);
	
	return vec3(
		colorFinal,
		colorFinal,
		colorFinal
	);
}

vec3 SampleNoiseBVertical(
	vec2 uv
)
{
	const float waveCount = c_noiseBWaveCount * c_heightToWidth;
	
	float x = uv.x;
	float y = uv.y;
	
	float wave =  c_noiseMaxAmplitude * sin(
		y * waveCount - TIME
	);
	
	vec2 offset = vec2(
		TIME * c_noiseBWaveSpeedY,
		TIME * c_noiseBWaveSpeedX
	);
	float colorNoise = texture(
		textureNoise,
		uv + offset
	).r * wave;
	
	float colorRemap = 	Remap(
		colorNoise,
		-1.0f,
		1.0f,
		c_noiseCutOffHeight,
		1.0f
	);
	
	float colorFinal = step(
		colorRemap,
		x + c_noiseWaveDepthOffset
	);
	
	return vec3(
		colorFinal,
		colorFinal,
		colorFinal
	);
}

vec3 SampleNoiseCHorizontal(
	vec2 uv
)
{
	float x = uv.x;
	float y = uv.y;
	
	float wave =  c_noiseMaxAmplitude * sin(
		x * c_noiseCWaveCount - TIME
	);
	
	vec2 offset = vec2(
		TIME * c_noiseCWaveSpeedX,
		TIME * c_noiseCWaveSpeedY
	);
	float colorNoise = texture(
		textureNoise,
		uv + offset
	).r * wave;
	
	float colorRemap = 	Remap(
		colorNoise,
		-1.0f,
		1.0f,
		c_noiseCutOffWidth,
		1.0f
	);
	
	float colorFinal = step(
		colorRemap,
		y + c_noiseWaveHeightOffset
	);
	
	return vec3(
		colorFinal,
		colorFinal,
		colorFinal
	);
}

vec3 SampleNoiseCVertical(
	vec2 uv
)
{
	const float waveCount = c_noiseCWaveCount * c_heightToWidth;
	
	float x = uv.x;
	float y = uv.y;
	
	float wave =  c_noiseMaxAmplitude * sin(
		y * waveCount - TIME
	);
	
	vec2 offset = vec2(
		TIME * c_noiseCWaveSpeedY,
		TIME * c_noiseCWaveSpeedX
	);
	float colorNoise = texture(
		textureNoise,
		uv + offset
	).r * wave;
	
	float colorRemap = 	Remap(
		colorNoise,
		-1.0f,
		1.0f,
		c_noiseCutOffHeight,
		1.0f
	);
	
	float colorFinal = step(
		colorRemap,
		x + c_noiseWaveDepthOffset
	);
	
	return vec3(
		colorFinal,
		colorFinal,
		colorFinal
	);
}

void fragment()
{
	vec4 colorMain = texture(
		textureWave, 
		UV
	);	
		
	const float negativeU = -UV.x;
	const float negativeV = -UV.y;
	const float oneMinusU = 1.0f - UV.x;
	const float oneMinusV = 1.0f - UV.y;
	
	const vec2 negativeUOneMinusV = vec2(
		negativeU,
		oneMinusV
	);
	const vec2 oneMinusUNegativeV = vec2(
		oneMinusU,
		negativeV
	);
	
	const vec3 colorNoiseA = SampleNoiseAHorizontal(
		UV
	) + SampleNoiseAHorizontal(
		negativeUOneMinusV
	) + SampleNoiseAVertical(
		UV
	) + SampleNoiseAVertical(
		oneMinusUNegativeV
	);

	const vec3 colorNoiseB = SampleNoiseBHorizontal(
		UV
	) + SampleNoiseBHorizontal(
		negativeUOneMinusV
	) + SampleNoiseBVertical(
		UV
	) + SampleNoiseBVertical(
		oneMinusUNegativeV
	);
	
	const vec3 colorNoiseC = SampleNoiseCHorizontal(
		UV
	) + SampleNoiseCHorizontal(
		negativeUOneMinusV
	) + SampleNoiseCVertical(
		UV
	) + SampleNoiseCVertical(
		oneMinusUNegativeV
	);
		
	const vec3 colorNoiseFinal = colorNoiseA + colorNoiseB + colorNoiseC;
	
	vec3 colorNoiseAdditive = texture(
		textureNoise,
		UV	+ TIME * c_noiseColorSpeed
	).rgb;
	
	float hdrModifier = dot(
		vec3(
			1.0f
		),
		color.rgb
	);
	const float saturationMultiplier = 0.33f;
	vec3 colorSaturated = vec3(
		hdrModifier
	) * saturationMultiplier;
	vec3 colorSaturationMix = mix(
		colorSaturated,
		color.rgb,
		3.0f
	);
	
	vec3 colorFinal = clamp(
		colorMain.rgb + colorNoiseFinal,
		0.0f, 
		1.0f
	) * colorSaturationMix;
	
	float mask = texture(
		textureMask,
		UV
	).a;
	float alpha = mask * max(
		colorMain.a, 
		colorNoiseFinal.r
	);
	
	COLOR = vec4(
		colorFinal.rgb, 
		alpha
	);
}