﻿#version 330

in vec3 vPosition;

void main()
{
	gl_Position = vec4(vPosition, 1);
	//gl_Position = vec4(vPosition, 0, 1);
}
