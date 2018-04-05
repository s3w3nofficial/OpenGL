#version 330

in vec3 position;
in vec2 uv;

out vec4 pass_color;
out vec2 pass_uv;

uniform mat4 transformation;
uniform mat4 projection;
uniform mat4 view;

void main(void) {
    gl_Position = projection * view * transformation * vec4(position, 1.0);
    
    vec3 color3 = vec3(0.25) + vec3(0.75) * (gl_VertexID / 6) / 4.0;
    
    pass_color = vec4(color3, 1);
    pass_uv = uv;
}
