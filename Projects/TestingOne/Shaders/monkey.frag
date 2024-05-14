
#version 330 core

in vec3 fragNormal;
in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform float time;

out vec4 fragColor;

void main(){
    vec4 texColor = texture(texture0, fragTexCoord);

    if (texColor.a < 0.1)
        discard;

    fragColor = texColor;
}
