shader_type spatial;
render_mode shadows_disabled, ambient_light_disabled, unshaded, skip_vertex_transform;

uniform lowp vec4 color : source_color = vec4(1.0f, 1.0f, 1.0f, 1.0f);
uniform lowp float outline : hint_range(0.0f, 1.0f) = 0.1f;

void vertex()
{
	vec4 modelNormal = MODEL_MATRIX * vec4(NORMAL, 0.0f);
	vec4 modelVertex = MODEL_MATRIX * vec4(VERTEX, 1.0f);
	modelVertex += outline * modelNormal;
	
	vec4 viewNormal = VIEW_MATRIX * modelNormal;
	vec4 viewVertex = VIEW_MATRIX * modelVertex;
	
    NORMAL = normalize(
		viewNormal.xyz
	);	
	VERTEX = viewVertex.xyz;
}

void fragment() 
{	
	ALBEDO = color.rgb;
}