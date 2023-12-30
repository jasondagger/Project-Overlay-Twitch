shader_type canvas_item;

uniform sampler2D textureMain;

void fragment()
{
	vec4 colorMain = texture(
		textureMain, 
		UV
	);
	
	COLOR = colorMain.rgba;
}