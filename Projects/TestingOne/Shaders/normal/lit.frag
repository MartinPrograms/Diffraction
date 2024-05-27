
#version 400 core
#define MAX_LIGHTS 1

in vec3 fragNormal;
in vec2 fragTexCoord;
in vec3 fragPos;
in vec4 fragPosLightSpaces[MAX_LIGHTS];

in vec3 TangentLightPos[MAX_LIGHTS];
in vec3 TangentViewPos;
in vec3 TangentFragPos;

uniform sampler2D texture0;

uniform int useNormalMap;
uniform sampler2D normalMap;

uniform float time;

// Lighting properties
uniform vec3 ambientColor;
uniform vec3 lightColor[MAX_LIGHTS];
uniform vec3 lightPos[MAX_LIGHTS];
uniform float lightIntensity[MAX_LIGHTS];
uniform int lightType[MAX_LIGHTS]; // 0 = point, 1 = directional
uniform float lightRange[MAX_LIGHTS]; // in units, so the light will be at 0 intensity at this distance
uniform int lightCount;

uniform int hasDirectionalLight;
uniform int hasPointLight;

// Shadow properties
uniform sampler2D shadowMaps[MAX_LIGHTS];
uniform mat4 shadowMatrices[MAX_LIGHTS];

uniform samplerCube shadowCubeMap[MAX_LIGHTS];
uniform float far_plane[MAX_LIGHTS];
uniform float lightFalloff[MAX_LIGHTS];

uniform float shadowBias[MAX_LIGHTS];

// Material properties
uniform vec3 materialColor;
uniform float materialSpecularStrength;
uniform float materialSpecularExponent; // 32 is a good value for this

// Some camera properties
uniform vec3 cameraPos;

out vec4 fragColor;

vec2 poissonDisk[16] = vec2[](vec2(-0.94201624, -0.39906216), vec2(0.94558609, -0.76890725), vec2(-0.094184101, -0.92938870), vec2(0.34495938, 0.29387760), vec2(-0.91588581, 0.45771432), vec2(-0.81544232, -0.87912464), vec2(-0.38277543, 0.27676845), vec2(0.97484398, 0.75648379), vec2(0.44323325, -0.97511554), vec2(0.53742981, -0.47373420), vec2(-0.26496911, -0.41893023), vec2(0.79197514, 0.19090188), vec2(-0.24188840, 0.99706507), vec2(-0.81409955, 0.91437590), vec2(0.19984126, 0.78641367), vec2(0.14383161, -0.14100790));

// Returns a random number based on a vec3 and an int.
float random(vec3 seed, int i) {
    vec4 seed4 = vec4(seed, i);
    float dot_product = dot(seed4, vec4(12.9898, 78.233, 45.164, 94.673));
    return fract(sin(dot_product) * 43758.5453);
}

float CalculateDirectionalShadow(sampler2D shadowMap, vec4 fragPosLightSpace) {
    float visibility = 1.0; // Fully visible by default

    vec4 projCoords = fragPosLightSpace;
    projCoords /= projCoords.w;
    projCoords = projCoords * 0.5 + 0.5;
    projCoords.z -= 0.00001; // Bias

    for(int i = 0; i < 16; i++) {
        vec2 offset = poissonDisk[i];
        vec4 offsetCoords = projCoords;
        offsetCoords.xy += offset / 5000.0;
        float shadow = texture(shadowMap, offsetCoords.xy).r;
        if(projCoords.z > shadow) {
            visibility -= 1.0 / 16.0;
        }
    }

    return visibility;
}

vec3 sampleOffsetDirections[20] = vec3[](vec3(1, 1, 1), vec3(1, -1, 1), vec3(-1, -1, 1), vec3(-1, 1, 1), vec3(1, 1, -1), vec3(1, -1, -1), vec3(-1, -1, -1), vec3(-1, 1, -1), vec3(1, 1, 0), vec3(1, -1, 0), vec3(-1, -1, 0), vec3(-1, 1, 0), vec3(1, 0, 1), vec3(-1, 0, 1), vec3(1, 0, -1), vec3(-1, 0, -1), vec3(0, 1, 1), vec3(0, -1, 1), vec3(0, -1, -1), vec3(0, 1, -1));

float CalculatePointShadow(int index, float far_plane, vec3 lightPos, vec3 fragPos) {
    vec3 fragToLight = fragPos - lightPos;
    float distance = length(fragToLight);
    fragToLight = normalize(fragToLight);

    int staticIndex = index; // something really weird is happening here, when calling shadowCubeMap[0] or [1] it works fine, but when using a variable it doesn't work, why?

    // so i am going to have to pull some bad code here, (big if statement)
    float closestDepth = 0.0;

    if (staticIndex == 0) {
        closestDepth = texture(shadowCubeMap[0], fragToLight).r;
    }



    // 4 should be enough for now, i should be using ssbo's for this, but i am too concerned about macos compatibility
    // which is understandable, as half the development i do is on a mac
    // i might also just switch to metal, but that would be an entire new backend, and this is just a hobby project
    // and also i have never used metal before, so i would have to learn it first
    // it does seem like a good idea though, as it is more modern and has better performance
    // although this engine is pretty intertwined with opengl, so it would be a lot of work to switch, might even be easier to just start a new project
    // maybe continue this one as a sort of legacy project, and start a new one with metal & opengl (4.6), never vulkan though that is too much work

    closestDepth *= far_plane;

    float currentDepth = distance;

    float bias = shadowBias[staticIndex];
    float shadowMapValue = closestDepth;

    float shadow = currentDepth - bias > shadowMapValue ? 0.0 : 1.0;

    // Quadratic fall off, based on the lightFalloff
    float distanceToLight = length(fragPos - lightPos);
    float falloff = lightFalloff[staticIndex];
    float shadowFalloff = 1.0 - pow(distanceToLight / far_plane, falloff);

    shadow *= shadowFalloff;

    return shadow;
}

void main() {
    vec4 texColor = texture(texture0, fragTexCoord);
    vec4 normalFromMap = texture(normalMap, fragTexCoord);

    // First we start with the ambient light
    vec3 result = ambientColor * texColor.rgb;
    vec3 normal = fragNormal;//normalize(normalFromMap.rgb * 2.0 - 1.0);

    if(useNormalMap == 1) {
        normal = normalize(normalFromMap.rgb * 2.0 - 1.0);
    }

    vec3 lightPass = vec3(0.0);
    float beshadowed = 1.0;
    for(int i = 0; i < lightCount; i++) {
        
        if(lightType[i] == 0 && hasDirectionalLight == 1) { // Directional light
            
            vec3 lightDir = normalize(TangentLightPos[i] - TangentFragPos);
            float diff = max(dot(normal, lightDir), 0.0);
            vec3 diffuse = lightColor[i] * diff * texColor.rgb;

        // Specular lighting
            vec3 viewDir = normalize(TangentViewPos - TangentFragPos);
            // Blinn-Phong
            vec3 halfwayDir = normalize(lightDir + viewDir);
            float spec = pow(max(dot(normal, halfwayDir), 0.0), materialSpecularExponent);
            vec3 specular = lightColor[i] * spec * materialSpecularStrength;

            float shadow = 1.0;
            shadow = CalculateDirectionalShadow(shadowMaps[i], fragPosLightSpaces[0]);

            beshadowed = shadow;

            lightPass += (diffuse + specular); // shadows are done after the loop, because if we layer them on top of each other, it will be too dark
            
        }

        
        if(lightType[i] == 1 && hasPointLight == 1) { // Point light
            float shadow = CalculatePointShadow(i, far_plane[i], lightPos[i], fragPos);
            beshadowed = shadow;

            vec3 lightDir = normalize(TangentLightPos[i] - TangentFragPos);
            float diff = max(dot(normal, lightDir), 0.0);
            vec3 diffuse = lightColor[i] * diff * texColor.rgb;

            // Specular lighting
            vec3 viewDir = normalize(TangentViewPos - TangentFragPos);
            // Blinn-Phong
            vec3 halfwayDir = normalize(lightDir + viewDir);
            float spec = pow(max(dot(normal, halfwayDir), 0.0), materialSpecularExponent);
            vec3 specular = lightColor[i] * spec * materialSpecularStrength;

            lightPass += (diffuse + specular);
            //lightPass = vec3(beshadowed);
        }
        
        
    }

    // Shadows are applied after the loop
    if(useNormalMap == 1) {
        result += lightPass * beshadowed;
    }

    vec3 finalColor = result;
    fragColor = vec4(finalColor, 1.0);
}
