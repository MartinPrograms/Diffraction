
#version 330 core
#define MAX_LIGHTS 1

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texCoord;

// Normal mapping
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

uniform mat4 model;
uniform mat4 view;
uniform vec3 cameraPos;
uniform mat4 projection;
uniform mat4 shadowMatrices[MAX_LIGHTS];
uniform vec3 lightPosAbs[MAX_LIGHTS];

out vec3 fragNormal;
out vec2 fragTexCoord;
out vec3 fragPos;
out vec4 fragPosLightSpaces[MAX_LIGHTS];

out vec3 TangentLightPos[MAX_LIGHTS];
out vec3 TangentViewPos;
out vec3 TangentFragPos;

void main()
{
    gl_Position = projection * view * model * vec4(position, 1.0);
    fragNormal = vec3(model * vec4(normal, 0.0));
    fragTexCoord = texCoord;
    fragPos = vec3(model * vec4(position, 1.0));

    for(int i = 0; i < MAX_LIGHTS; ++i){
        fragPosLightSpaces[i] = shadowMatrices[i] * vec4(fragPos, 1.0);
    }
    
    // tangent stuff:

    mat3 normalMatrix = transpose(inverse(mat3(model)));
    vec3 T = normalize(normalMatrix * tangent);
    vec3 N = normalize(normalMatrix * normal);
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);

    mat3 TBN = transpose(mat3(T, B, N));
    for (int i = 0; i < MAX_LIGHTS; ++i){
        TangentLightPos[i] = TBN * lightPosAbs[i];
    }
    TangentViewPos = TBN * cameraPos;
    TangentFragPos = TBN * fragPos;

    // yay we're done
}
