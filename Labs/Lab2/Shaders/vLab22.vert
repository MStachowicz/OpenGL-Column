#version 330

//in vec3 vPosition;
//in vec3 vColour;
//
//out vec4 oColour;
//
//void main()
//{
//	gl_Position = vec4(vPosition, 1);
//	oColour = vec4(vColour, 1);
//}

uniform mat4 uModel;
uniform mat4 uView;

in vec3 vPosition; 
in vec3 vColour;

out vec4 oColour;

void main() 
{ 
//gl_Position = vec4(vPosition, 1) * uModel; 
gl_Position = vec4(vPosition, 1) * uModel * uView;
oColour = vec4(vColour, 1); 
}