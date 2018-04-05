#version 330

in vec4 pass_color;
in vec2 pass_uv;

out vec4 out_Color;

uniform sampler2D sampler;

void main(void)
{
    vec4 pixelColor = texture(sampler, pass_uv);
    out_Color = pass_color * pixelColor;
}
