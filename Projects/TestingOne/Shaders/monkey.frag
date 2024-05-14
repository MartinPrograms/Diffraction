
#version 330 core

in vec3 fragNormal;
in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform float time;

out vec4 fragColor;

void main(){
    vec4 texColor = texture(texture0, fragTexCoord);


    vec3 rainbow = vec3(1.0 - abs(sin(time)), 1.0 - abs(sin(time + 2.0)), 1.0 - abs(sin(time + 4.0)));

    float border = 0.04;
    float borderEffect = 1;
    if(fragTexCoord.x < border){
        texColor.rgb = mix(texColor.rgb, rainbow, borderEffect);
        texColor.a = 1.0;
    }
    if(fragTexCoord.x > 1.0 - border){
        texColor.rgb = mix(texColor.rgb, rainbow, borderEffect);
                texColor.a = 1.0;

    }
    if(fragTexCoord.y < border){
        texColor.rgb = mix(texColor.rgb, rainbow, borderEffect);
                texColor.a = 1.0;

    }
    if(fragTexCoord.y > 1.0 - border){
        texColor.rgb = mix(texColor.rgb, rainbow, borderEffect);
                texColor.a = 1.0;
    }

    if (texColor.a < 0.1)
        discard;

    fragColor = texColor;
}
