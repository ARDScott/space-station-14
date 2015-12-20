#define RADIUS 5
#define KERNEL_SIZE (RADIUS * 2 + 1)
 
uniform vec2 weights_offsets[KERNEL_SIZE];
uniform vec2 weights_offsets0;
uniform vec2 weights_offsets1;
uniform vec2 weights_offsets2;
uniform vec2 weights_offsets3;
uniform vec2 weights_offsets4;
uniform vec2 weights_offsets5;
uniform vec2 weights_offsets6;
uniform vec2 weights_offsets7;
uniform vec2 weights_offsets8;
uniform vec2 weights_offsets9;
uniform vec2 weights_offsets10;

uniform sampler2D colorMapTexture;

vec4 GaussianBlurVertical()
{
    vec4 color = vec4(0,0,0,0);
    
    vec2 weights_offsets[KERNEL_SIZE] = vec2[KERNEL_SIZE]
    (
		weights_offsets0,
		weights_offsets1,
		weights_offsets2,
		weights_offsets3,
		weights_offsets4,
		weights_offsets5,
		weights_offsets6,
		weights_offsets7,
		weights_offsets8,
		weights_offsets9,
		weights_offsets10
	);
    
    for (int i = 0; i < KERNEL_SIZE; ++i)
        color += texture2D(colorMapTexture, vec2(gl_TexCoord[0].x, gl_TexCoord[0].y + weights_offsets[i].y)) * weights_offsets[i].x;
        
    return color;
}



void main()
{	
 	gl_FragColor = GaussianBlurVertical();
}
