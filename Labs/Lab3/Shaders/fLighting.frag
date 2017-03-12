#version 330
uniform vec4 uLightPosition;
uniform vec4 uEyePosition;

in vec4 oNormal;
in vec4 oSurfacePosition;

out vec4 FragColour;
void main() 
{
	// Diffuse light
    //vec4 lightDir = normalize(uLightPosition - oSurfacePosition);
    //FragColour = vec4(vec3(diffuseFactor), 1);
    
	// Specular light + diffuse
	vec4 lightDir = normalize(uLightPosition - oSurfacePosition);
    
	float diffuseFactor = max(dot(oNormal, lightDir), 0);
    
	vec4 eyeDirection = normalize(uEyePosition - oSurfacePosition);
    vec4 reflectedVector = reflect(-lightDir, oNormal);

    float specularFactor = pow(max(dot( reflectedVector, eyeDirection), 0.0), 20);



	FragColour = vec4(vec3(diffuseFactor + specularFactor), 1);
}


