#version 330
uniform vec4 uEyePosition;

in vec4 oNormal;
in vec4 oSurfacePosition;

out vec4 FragColour;


struct LightProperties {
vec4 Position;
vec3 AmbientLight;
vec3 DiffuseLight;
vec3 SpecularLight;
};
uniform LightProperties[] uLight;

struct MaterialProperties {
vec3 AmbientReflectivity;
vec3 DiffuseReflectivity;
vec3 SpecularReflectivity;
float Shininess;
};
uniform MaterialProperties uMaterial;


void main() 
{   
	vec4 eyeDirection = normalize(uEyePosition - oSurfacePosition);

//for(int i = 0; i < 3; ++i) 
//	{
//UNROLLED FOR LOOP, does not work in a loop
        vec4 lightDir = normalize(uLight[0].Position - oSurfacePosition);
        vec4 reflectedVector = reflect(-lightDir, oNormal);
        float diffuseFactor = max(dot(oNormal, lightDir), 0);
        float specularFactor = pow(max(dot( reflectedVector, eyeDirection), 0.0), uMaterial.Shininess);
        FragColour = FragColour + vec4(
		uLight[0].AmbientLight * uMaterial.AmbientReflectivity +
	    uLight[0].DiffuseLight * uMaterial.DiffuseReflectivity * diffuseFactor + 
		uLight[0].SpecularLight * uMaterial.SpecularReflectivity * specularFactor, 1);

		lightDir = normalize(uLight[1].Position - oSurfacePosition);
        reflectedVector = reflect(-lightDir, oNormal);
        diffuseFactor = max(dot(oNormal, lightDir), 0);
        specularFactor = pow(max(dot( reflectedVector, eyeDirection), 0.0), uMaterial.Shininess);
        FragColour = FragColour + vec4(
		uLight[1].AmbientLight * uMaterial.AmbientReflectivity +
	    uLight[1].DiffuseLight * uMaterial.DiffuseReflectivity * diffuseFactor + 
		uLight[1].SpecularLight * uMaterial.SpecularReflectivity * specularFactor, 1);


		lightDir = normalize(uLight[2].Position - oSurfacePosition);
        reflectedVector = reflect(-lightDir, oNormal);
        diffuseFactor = max(dot(oNormal, lightDir), 0);
        specularFactor = pow(max(dot( reflectedVector, eyeDirection), 0.0), uMaterial.Shininess);
        FragColour = FragColour + vec4(
		uLight[2].AmbientLight * uMaterial.AmbientReflectivity +
	    uLight[2].DiffuseLight * uMaterial.DiffuseReflectivity * diffuseFactor + 
		uLight[2].SpecularLight * uMaterial.SpecularReflectivity * specularFactor, 1);

   // }



}