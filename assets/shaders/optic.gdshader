shader_type spatial;

uniform float dist = 0.001; 
uniform float scale = 1.0;
uniform float glow  = 1.0; 
uniform sampler2D reticle_texture : source_color, repeat_disable;
uniform vec4 reticle_color: source_color; 
uniform vec4 glass_color: source_color; 

void fragment() {

	vec3 vm3 = INV_VIEW_MATRIX[3].xyz;
	vec3 mm3 = MODEL_MATRIX[3].xyz;
	float len = length(vm3 - mm3);

	vec3 view = (vec4(-normalize(VIEW), 1.0) * VIEW_MATRIX).xyz;
	float s = scale * 10.0;

	vec3 m1 = view * vec3(dist);
	vec3 tm = (vec4(m1, 1) * MODEL_MATRIX).xyz;

	vec3 offset = tm + vec3(0.5);
	vec3 clmp = clamp(offset, vec3(0), vec3(1));
	vec3 clmp0 = vec3(1.0, 1.0, 1.0) - clmp;

	vec4 tex = texture(reticle_texture, clmp0.xy); 
	
	vec3 reticle = (tex.rgb  * reticle_color.rgb); 
	
	EMISSION = reticle * glow; 
	ALBEDO = glass_color.rgb;
	ALPHA = tex.a + glass_color.b; 
}