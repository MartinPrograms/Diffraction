
#version 330 core
#define MAX_LIGHTS 16

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

float CalculatePointShadow(samplerCube shadowMap, float far_plane, vec3 lightPos, vec3 fragPos, float bias){
    vec3 fragToLight = fragPos - lightPos;
    float closestDepth = texture(shadowMap, fragToLight).r;
    closestDepth *= far_plane;
    float currentDepth = length(fragToLight);
    float shadow = currentDepth - bias > closestDepth ? 0.0 : 1.0;
    return shadow;
}

float CalculateDirectionalShadow(sampler2D shadowMap, mat4 shadowMatrix, vec3 fragPos, float ignore, vec4 fragPosLightSpace){
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;
    if (projCoords.z > 1.0){
        return 0.0;
    }
    float shadow = 0.0;
    
    float bias = max(0.05 * (1.0 - dot(fragNormal, normalize(lightPos[0] - fragPos))), 0.005);

    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for (int x = -1; x <= 1; x++){
        for (int y = -1; y <= 1; y++){
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;

            shadow += currentDepth - bias > pcfDepth ? 0.0 : 1.0;
        }
    }

    shadow /= 9.0;

    return shadow;
}

void main(){
    vec4 texColor = texture(texture0, fragTexCoord);

    if (texColor.a < 0.1){
        //discard;
    }

    vec3 normal = normalize(fragNormal);
    // First we start with the ambient light
    vec3 result = ambientColor * texColor.rgb;

    // A lighttype 0 is a directional light, light type 1 is a point light
    for (int i = 0; i < lightCount; i++){
        if (lightType[i] == 1){
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
            //float shadow = CalculatePointShadow(shadowCubeMap[i], far_plane[i], lightPos[i], fragPos, shadowBias[i]);
            //result *= shadow;

            result += (diffuse * lightColor[i] * texColor * intensity);
        }

        if (lightType[i] == 0){
            // Directional light
            vec3 lightDir = -lightPos[i];
            lightDir = normalize(lightDir);
            float diffuse = max(dot(normal, lightDir), 0.0);
            vec3 texColor = texture(texture0, fragTexCoord).rgb * materialColor;

            // Specular
            vec3 viewDir = normalize(cameraPos - fragPos);
            vec3 reflectDir = reflect(-lightDir, normal);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), materialSpecularExponent);


            // Shadows
            vec4 fragPosLightSpace = fragPosLightSpaces[i];
            float shadow = CalculateDirectionalShadow(shadowMaps[i], shadowMatrices[i], fragPos, shadowBias[i], fragPosLightSpace);
            
            result += (diffuse * lightColor[i] * texColor * lightIntensity[i] * shadow);
        }
    }

    vec3 finalColor = result;
    fragColor = vec4(finalColor, 1.0);
}
