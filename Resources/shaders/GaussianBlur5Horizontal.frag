#define RADIUS 5
#define KERNEL_SIZE (RADIUS * 2 + 1)

uniform sampler2D colorMap;
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
varying vec2 TexCoord;

vec4 GaussianBlurHorizontal()
{
   vec4 color = vec4(0,0,0,0);
    
    for (int i = 0; i < KERNEL_SIZE; ++i)
        color += texture2D(colorMap, vec2(TexCoord.x + weights_offsets[i].y, TexCoord.y)) * weights_offsets[i].x;
        
    return color;
}
void main()
{
	vec2 weights_offsets[KERNEL_SIZE] =
    {
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
	};

   gl_FragColor = GaussianBlurHorizontal();
}

