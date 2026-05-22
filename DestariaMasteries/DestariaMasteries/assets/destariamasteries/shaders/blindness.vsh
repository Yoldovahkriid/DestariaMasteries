#version 330 core

layout (location = 0) in vec3 position;

void main() 
{
	// Directly pass the quad positions to the pipeline
	gl_Position = vec4(position, 1.0);
}