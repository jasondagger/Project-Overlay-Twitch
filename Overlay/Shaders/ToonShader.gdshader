shader_type spatial;
render_mode shadows_disabled, ambient_light_disabled, unshaded;

uniform float minimumBrightness : hint_range(0.0f, 1.0f) = 0.0f;
uniform float maximumBrightness : hint_range(0.0f, 1.0f)= 1.0f;
uniform vec4 color : source_color = vec4(1.0f, 1.0f, 1.0f, 1.0f);
uniform sampler2D colorRamp : hint_default_white, filter_nearest;

const vec3 c_inverseLightDirection = vec3(0.0f, 0.5f, -0.5f);
const float c_rimPower = 4.0f;

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

void fragment() 
{
	vec4 worldNormal = normalize(
		INV_VIEW_MATRIX *
		vec4(
			NORMAL,
			0.0f
		)
	);
	float normalDotLightWorld = dot(
		normalize(
			c_inverseLightDirection
		),
		worldNormal.xyz
	);
	float remappedNormalDotLightWorld = Remap(
		normalDotLightWorld,
		-1.0f,
		1.0f, 
		minimumBrightness,
		maximumBrightness
	);	
	vec4 toonColor = texture(
		colorRamp,
		vec2(
			remappedNormalDotLightWorld,
			0.5f
		)
	) * color;
	
	float normalDotCamera = dot(
		normalize(
			VIEW
		),
		NORMAL
	);
	
	float normalDotLightModel = dot(
		normalize(
			c_inverseLightDirection
		),
		NORMAL
	);
	float remappedNormalDotLightModel = Remap(
		normalDotLightModel,
		-1.0f,
		1.0f, 
		0.2f,
		0.8f
	);	
	
	float rimIntensity = pow(1.0f - normalDotCamera, c_rimPower);
	vec4 rimColor = remappedNormalDotLightModel * rimIntensity * color;
	vec4 colorFinal = toonColor + rimColor;
	
	ALBEDO = toonColor.xyz;
}