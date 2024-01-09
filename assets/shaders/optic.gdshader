shader_type spatial;

uniform bool flip_x = false; 
uniform bool flip_y = false; 
uniform float dist = 0.001; 
uniform float scale = 1.0;
uniform float glow  = 1.0; 
uniform sampler2D reticle_texture : source_color, repeat_disable;
uniform vec4 reticle_color: source_color; 
uniform vec4 glass_color: source_color; 


void fragment() {

	vec3 mult = vec3(1);

	mat4 vm = VIEW_MATRIX; 
	vec3 view = (vec4(-normalize(VIEW * mult), 1.0) * vm).xyz;
	float s = scale * 10.0;
	
	
	vec3 m1 = view * vec3(dist) * vec3(1, 1, 1);
	vec3 tm = (vec4(m1, 1) * MODEL_MATRIX).xyz;

	vec3 offset = tm + vec3(0.5);
	vec3 clmp = clamp(offset, vec3(0), vec3(1));
	vec3 clmp0 = vec3(1.0, 1.0, 1.0) - clmp;
	
	vec2 uv = vec2(clmp0.x, clmp0.y); 
	if (flip_x) { 
		uv.x = 1.0-clmp0.x; 
	}
	
	if (flip_y) { 
		uv.y = 1.0-clmp0.y; 
	}
	
	vec4 tex = texture(reticle_texture, vec2(1.0-clmp0.x, clmp0.y)); 
	vec3 reticle = (tex.rgb  * reticle_color.rgb); 
	
	EMISSION = reticle * glow; 
	ALBEDO = glass_color.rgb + reticle;
	ALPHA = tex.a + glass_color.b; 
}