
#version 330 core
#define MAX_LIGHTS 16

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 shadowMatrices[MAX_LIGHTS];

out vec3 fragNormal;
out vec2 fragTexCoord;
out vec3 fragPos;
out vec4 fragPosLightSpaces[MAX_LIGHTS];

void main()
{
    gl_Position = projection * view * model * vec4(position, 1.0);
    fragNormal = normal;
    fragTexCoord = texCoord;
    fragPos = vec3(model * vec4(position, 1.0));
    for(int i = 0; i < MAX_LIGHTS; ++i){
        fragPosLightSpaces[i] = shadowMatrices[i] * vec4(fragPos, 1.0);
    }
}
