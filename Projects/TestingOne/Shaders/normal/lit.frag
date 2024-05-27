
#version 400 core
#define MAX_LIGHTS 6

in vec3 fragNormal;
in vec2 fragTexCoord;
in vec3 fragPos;
in vec4 fragPosLightSpaces[MAX_LIGHTS];

uniform sampler2D texture0;

uniform float time;

// Lighting properties
uniform vec3 ambientColor;
uniform vec3 lightColor[MAX_LIGHTS];
uniform vec3 lightPos[MAX_LIGHTS];
uniform float lightIntensity[MAX_LIGHTS];
uniform int lightType[MAX_LIGHTS]; // 0 = point, 1 = directional
uniform float lightRange[MAX_LIGHTS]; // in units, so the light will be at 0 intensity at this distance
uniform int lightCount;

// Shadow properties
uniform sampler2D shadowMaps[MAX_LIGHTS];
uniform mat4 shadowMatrices[MAX_LIGHTS];

uniform samplerCube shadowCubeMap[MAX_LIGHTS];
uniform float far_plane[MAX_LIGHTS];

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
    projCoords.z -= 0.000001; // Bias

    for (int i = 0; i < 16; i++) {
        vec2 offset = poissonDisk[i];
        vec4 offsetCoords = projCoords;
        offsetCoords.xy += offset / 5000.0;
        float shadow = texture(shadowMap, offsetCoords.xy).r;
        if (projCoords.z > shadow) {
            visibility -= 1.0 / 16.0;
        }
    }

    return visibility;
}


float CalculatePointShadow(int index, float far_plane, vec3 lightPos, vec3 fragPos) {
    vec3 fragToLight = fragPos - lightPos;
    float closestDepth = texture(shadowCubeMap[index], vec3(1.0,1.0,1.0)).r;
    closestDepth *= far_plane;
    
    float currentDepth = length(fragToLight);

    float bias = 0.05;
    float shadow = currentDepth - bias > closestDepth ? 0.0 : 1.0;
    
    return shadow;
}

void main() {
    vec4 texColor = texture(texture0, fragTexCoord);

    vec3 normal = normalize(fragNormal);
    // First we start with the ambient light
    vec3 result = ambientColor * texColor.rgb;

    // A lighttype 0 is a directional light, light type 1 is a point light
    for(int i = 0; i < lightCount; i++) {
        if(lightType[i] == 1) {
            // Point light

            vec3 lightDir = lightPos[i] - fragPos;
            float distance = length(lightDir);
            lightDir = normalize(lightDir);
            float attenuation = 1.0 - clamp(distance / lightRange[i], 0.0, 1.0);
            float intensity = lightIntensity[i] * attenuation;
            float diffuse = max(dot(normal, lightDir), 0.0);
            vec3 texColor = texture(texture0, fragTexCoord).rgb * materialColor;

            // Specular
            vec3 viewDir = normalize(cameraPos - fragPos);
            vec3 reflectDir = reflect(-lightDir, normal);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), materialSpecularExponent);

            // Shadows
            float shadow = 0.1; 
            shadow = CalculatePointShadow(i, far_plane[i], lightPos[i], fragPos);

            if (shadow < 0.2) {
                shadow = 0.2;
            }

            result += (diffuse * lightColor[i] * texColor + intensity * spec) * shadow;

        }

        if(lightType[i] == 0) {
            // Directional light
            vec3 lightDir = -lightPos[i];
            lightDir = normalize(lightDir);
            float diffuse = max(dot(normal, lightDir), 0.0);
            vec3 texColor = texture(texture0, fragTexCoord).rgb * materialColor;

            // Specular
            vec3 viewDir = normalize(cameraPos - fragPos);
            vec3 reflectDir = reflect(-lightDir, normal);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), materialSpecularExponent);
            float specular = materialSpecularStrength * spec;

            // Shadows
            vec4 fragPosLightSpace = fragPosLightSpaces[i];
            float shadow = CalculateDirectionalShadow(shadowMaps[i], fragPosLightSpace);

            if (shadow < 0.2) {
                shadow = 0.2;
            }

            result += (diffuse * lightColor[i] * texColor + specular) * shadow;
        }
    }

    vec3 finalColor = result;
    fragColor = vec4(finalColor, 1.0);
}
