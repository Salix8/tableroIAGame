shader_type spatial;
render_mode blend_add;
render_mode unshaded;
varying vec3 worldPos;
varying vec3 worldNormal;
uniform vec3 rim_color : source_color;
uniform float rim_power : hint_range(0.1, 20.0);
void vertex() {
	worldPos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	worldNormal = MODEL_NORMAL_MATRIX * NORMAL;
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec3 cameraDirection = normalize(CAMERA_POSITION_WORLD - worldPos);
	float rimWeight = pow(dot(worldNormal,cameraDirection),rim_power);
	float dif = clamp(1.0 - pow(rimWeight,rim_power),0.0,1.0);
	ALBEDO = rim_color;
	ALPHA = dif;
	// Called for every pixel the material is visible on.
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
