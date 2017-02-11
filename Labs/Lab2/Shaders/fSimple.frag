#version 330

//uniform vec4 uColour;

in vec4 oColour;

out vec4 FragColour;

void main()
{
	FragColour = oColour;
}