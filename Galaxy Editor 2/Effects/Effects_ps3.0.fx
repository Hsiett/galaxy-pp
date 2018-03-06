float4x4 MatrixTransform;
uniform extern texture ScreenTexture;    
uniform extern float2 TextureSize;
uniform extern float2 TargetPos;
uniform extern float2 TargetSize;
uniform extern float2 ScreenSize;
uniform extern int BoxType;
uniform extern bool Tiled;
uniform extern float4 TintColor;
uniform extern bool IsHalfTexture;
uniform extern bool IsBottomHalf;

const int BoxTypeSelection = 0;
const int BoxTypeCreateNew = 1;

//uniform bool DrawResizeDots;
//uniform extern float2 Boarder;//(TextureHeight/TargetWidth, TextureHeight/TargetHeight)

void SpriteVertexShader(inout float4 vColor : COLOR0, inout float2 texCoord : TEXCOORD0, inout float4 position : POSITION0)
{
	position = position;// mul(position, MatrixTransform);
}

sampler ScreenS = sampler_state
{
    Texture = <ScreenTexture>;    
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 DrawResizeBox(int2 targetPixel, int2 TopLeft)
{
	float4 returner = float4(1, 1, 1, 1);//Default fill color
	//Border
	/*if (targetPixel.x == TopLeft.x)
	{
		//Left
		if (targetPixel.y == TopLeft.y || targetPixel.y == TopLeft.y + 6)
		{
			returner = float4(0, 0, 0, 0);
		}
		else
			returner = float4(0, 0, 0, 1);
	}/*
	else if (targetPixel.y == TopLeft.y)
	{
		//Top
		if (targetPixel.x == TopLeft.x + 6)
			returner = float4(0, 0, 0, 0);
		else
			returner = float4(0, 0, 0, 1);
	}
	else if (targetPixel.x == TopLeft.x + 6)
	{
		//Right
		if (targetPixel.y == TopLeft.y + 6)
			returner = float4(0, 0, 0, 0);
		else
			returner = float4(0, 0, 0, 1);
	}
	else if (targetPixel.y == TopLeft.y + 6)
	{
		//Bottom
		returner = float4(0, 0, 0, 1);
	}*/
	return returner;
}

float4 SelectionBoxPS(float2 texCoord: TEXCOORD0) : COLOR
{
	int2 TargetPixel = (int2)(texCoord * TargetSize);
	float2 backgroundTextureCoord = (texCoord*TargetSize + TargetPos)/ScreenSize;
	/*float4 edgeColor = 1 - tex2D(ScreenS, backgroundTextureCoord); 
	edgeColor.a = 1;*/
	float4 edgeColor = tex2D(ScreenS, backgroundTextureCoord);
	/*edgeColor.r = step(edgeColor.r, 0.5);
	edgeColor.g = step(edgeColor.g, 0.5);
	edgeColor.b = step(edgeColor.b, 0.5);*/

	/*if (edgeColor.r > 0.5)
		edgeColor.r = 0;
	else
		edgeColor.r = 1;
	if (edgeColor.g > 0.5)
		edgeColor.g = 0;
	else
		edgeColor.g = 1;
	if (edgeColor.b > 0.5)
		edgeColor.b = 0;
	else
		edgeColor.b = 1;*/
	edgeColor.a = 1;
	float4 color = float4(0.1, 0.1, 0.1, 0);
	
	//Dotted line 4 pixels in, gray (105/255) . . . . . .
	//Selection dots 7x7 rectangle w/o corner pixels (black edge, white fill)
	
	//First calculate dotted line color
	
	if (TargetPixel.y == 3 && TargetPixel.x >= 3 && TargetPixel.x <= TargetSize.x - 4)
	{
		//Top border
		if (BoxType != BoxTypeSelection || TargetPixel.x%2 != 0)
			color = edgeColor;
	}
	else if (TargetPixel.x == 3 && TargetPixel.y >= 3 && TargetPixel.y <= TargetSize.y - 4)
	{
		//Left border
		if (BoxType != BoxTypeSelection || TargetPixel.y%2 != 0)
			color = edgeColor;
	}
	else if (TargetPixel.x == TargetSize.x - 4 && TargetPixel.y >= 3 && TargetPixel.y <= TargetSize.y - 4)
	{
		//Right border
		if (BoxType != BoxTypeSelection ||  TargetSize.x % 2 == TargetPixel.y % 2)
			color = edgeColor;
	}
	else if (TargetPixel.y == TargetSize.y - 4 && TargetPixel.x >= 3 && TargetPixel.x <= TargetSize.x - 4)
	{
		//Bottom border
		if (BoxType != BoxTypeSelection || TargetSize.y % 2 == TargetPixel.x % 2)
			color = edgeColor;
	}
	
	//Then calculate if this should be a resize box
	
	if (BoxType == BoxTypeSelection)
	{
		if (TargetPixel.x < 7)
		{
			//Left edge
			if (TargetPixel.y < 7)
			{
				//Top left
				color = DrawResizeBox(TargetPixel, int2(0, 0));
			}
			else if (TargetPixel.y >= TargetSize.y - 7)
			{
				//Bottom left
				color = DrawResizeBox(TargetPixel, int2(0, (int)TargetSize.y - 7));
			}
			else
			{
				//Middle left
				int y = (int)TargetSize.y / 2 - 3;
				if (TargetPixel.y >= y && TargetPixel.y < y + 7)
					color = DrawResizeBox(TargetPixel, int2(0, y));
			}
		}
		else if (TargetPixel.y < 7)
		{
			//Top edge
			if (TargetPixel.x >= TargetSize.x - 7)
			{
				//Top right
				color = DrawResizeBox(TargetPixel, int2((int)TargetSize.x - 7, 0));
			}
			else
			{
				//Middle top
				int x = (int)TargetSize.x / 2 - 3;
				if (TargetPixel.x >= x && TargetPixel.x < x + 7)
					color = DrawResizeBox(TargetPixel, int2(x, 0));
			}
		}
		else if (TargetPixel.y >= TargetSize.y - 7)
		{
			//Bottom edge
			if (TargetPixel.x >= TargetSize.x - 7)
			{
				//Bottom right
				color = DrawResizeBox(TargetPixel, int2((int)TargetSize.x - 7, (int)TargetSize.y - 7));
			}
			else
			{
				//Middle bottom
				int x = (int)TargetSize.x / 2 - 3;
				if (TargetPixel.x >= x && TargetPixel.x < x + 7)
					color = DrawResizeBox(TargetPixel, int2(x, (int)TargetSize.y - 7));
			}
		}
		else if (TargetPixel.x >= TargetSize.x - 7)
		{
			//Right edge
			//Middle right
			
			int y = (int)TargetSize.y / 2 - 3;
			if (TargetPixel.y >= y && TargetPixel.y < y + 7)
				color = DrawResizeBox(TargetPixel, int2((int)TargetSize.x - 7, y));
		}
	}
    return color;   
}

technique SelectionBox
{
    pass Pass0
    {
		//VertexShader = compile vs_2_a SpriteVertexShader();
        PixelShader = compile ps_2_a SelectionBoxPS();
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 HorizontalBorderPS(float2 texCoord: TEXCOORD0) : COLOR
{
	float2 targetCoord = texCoord * TargetSize;
	float2 edgeSize = float2(TextureSize.y, TextureSize.y);
	if (TargetSize.x < 2*edgeSize.x)
		edgeSize.x = TargetSize.x/2;
	if (TargetSize.y < 2*edgeSize.y)
		edgeSize.y = TargetSize.y/2;

	//First, we should check what region we are in.
	if (targetCoord.x < edgeSize.x)
	{
		if (targetCoord.y < edgeSize.y)
		{
			//Top left
			texCoord.x = targetCoord.x/(TextureSize.y*10);
			texCoord.y = targetCoord.y/TextureSize.y;
		}
		else if (TargetSize.y - targetCoord.y < edgeSize.y)
		{
			//Bottom left
			texCoord.x = targetCoord.x/(TextureSize.y*10) + 0.2;
			texCoord.y = (targetCoord.y - TargetSize.y + TextureSize.y)/TextureSize.y;
		}
		else
		{
			//Left border
			texCoord.x = targetCoord.x/(TextureSize.y*10) + 0.6;
			texCoord.y = ((targetCoord.y - edgeSize.y)/TextureSize.y)%1;
		}
	}
	else if (TargetSize.x - targetCoord.x < edgeSize.x)
	{
		if (targetCoord.y < edgeSize.y)
		{
			//Top right
			texCoord.x = (targetCoord.x - TargetSize.x + TextureSize.y)/(TextureSize.y*10) + 0.1;
			texCoord.y = targetCoord.y/TextureSize.y;
		}
		else if (TargetSize.y - targetCoord.y < edgeSize.y)
		{
			//Bottom right
			texCoord.x = (targetCoord.x - TargetSize.x + TextureSize.y)/(TextureSize.y*10) + 0.3;
			texCoord.y = (targetCoord.y - TargetSize.y + TextureSize.y)/TextureSize.y;
		}
		else
		{
			//Right border
			texCoord.x = (targetCoord.x - TargetSize.x + TextureSize.y)/(TextureSize.y*10) + 0.7;
			texCoord.y = ((targetCoord.y - edgeSize.y)/TextureSize.y)%1;
		}
	}
	else
	{
		if (targetCoord.y < edgeSize.y)
		{
			//Top border
			texCoord.x = ((targetCoord.x - edgeSize.x)/(TextureSize.y))%1;
			texCoord.y = targetCoord.y/TextureSize.y;
			//Need to rotate
			float temp = texCoord.x;
			texCoord.x = texCoord.y;
			texCoord.y = 1 - temp;
			//Ajust to region
			texCoord.x = texCoord.x/10 + 0.4;
		}
		else if (TargetSize.y - targetCoord.y < edgeSize.y)
		{
			//Bottom border
			texCoord.x = ((targetCoord.x - edgeSize.x)/(TextureSize.y))%1;
			texCoord.y = (targetCoord.y - TargetSize.y + TextureSize.y)/TextureSize.y;
			//Need to rotate
			float temp = texCoord.x;
			texCoord.x = texCoord.y;
			texCoord.y = 1 - temp;
			//Ajust to region
			texCoord.x = texCoord.x/10 + 0.5;
		}
		else
		{
			//Middle fill
			texCoord.x = (((targetCoord.x - edgeSize.x)/TextureSize.y)%1)/10 + 0.8;
			texCoord.y = ((targetCoord.y - edgeSize.y)/TextureSize.y)%1;
		}
	}
	
	if (IsHalfTexture)
	{
		texCoord.y /= 2;
		if (IsBottomHalf)
			texCoord.y += 0.5;
	}
	float4 color = tex2D(ScreenS, texCoord);
    return color * TintColor;    
}

technique HorizontalBorder
{
    pass Pass0
    {
		//VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_2_a HorizontalBorderPS();
    }
}



////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


float4 BorderPS(float2 texCoord: TEXCOORD0) : COLOR
{
	float texSizeY = TextureSize.y;
	if (IsHalfTexture)
	{
		texSizeY /= 2;
	}
	float2 TargetPixel = texCoord * TargetSize;
	float Texture1O8 = texSizeY / 8;
	float Texture1O4 = TextureSize.x / 4;

	if (TargetPixel.y < Texture1O8)
	{
		if (TargetPixel.x < Texture1O4)
		{
			//Top left
			//Normalize texture coordinate
			texCoord.x = TargetPixel.x/Texture1O4;
			texCoord.y = TargetPixel.y/Texture1O8;
			//Map to texture region
			texCoord.x = texCoord.x/4;
			texCoord.y = texCoord.y/8;
		}
		else if (TargetSize.x - TargetPixel.x < Texture1O4)
		{
			//Top right
			//Normalize texture coordinate
			texCoord.x = (TargetPixel.x - TargetSize.x)/Texture1O4 + 1;
			texCoord.y = TargetPixel.y/Texture1O8;
			//Map to texture region
			texCoord.x = (texCoord.x + 1)/4;
			texCoord.y = texCoord.y/8;
		}
		else
		{
			//Top border
			//Normalize texture coordinate
			texCoord.x = ((TargetPixel.x - Texture1O4)/TextureSize.x)%1;
			texCoord.y = TargetPixel.y/Texture1O8;
			//Map to texture region
			texCoord.x = texCoord.x;
			texCoord.y = (texCoord.y + 1)/8;
		}
	}
	else if (TargetSize.y - TargetPixel.y < Texture1O8)
	{
		if (TargetPixel.x < Texture1O4)
		{
			//Bottom left
			//Normalize texture coordinate
			texCoord.x = TargetPixel.x/Texture1O4;
			texCoord.y = (TargetPixel.y - TargetSize.y)/Texture1O8 + 1;
			//Map to texture region
			texCoord.x = (texCoord.x + 2)/4;
			texCoord.y = texCoord.y/8;
		}
		else if (TargetSize.x - TargetPixel.x < Texture1O4)
		{
			//Bottom right
			//Normalize texture coordinate
			texCoord.x = (TargetPixel.x - TargetSize.x)/Texture1O4 + 1;
			texCoord.y = (TargetPixel.y - TargetSize.y)/Texture1O8 + 1;
			//Map to texture region
			texCoord.x = (texCoord.x + 3)/4;
			texCoord.y = texCoord.y/8;
		}
		else
		{
			//Bottom border
			//Normalize texture coordinate
			texCoord.x = ((TargetPixel.x - Texture1O4)/TextureSize.x)%1;
			texCoord.y = (TargetPixel.y - TargetSize.y)/Texture1O8 + 1;
			//Map to texture region
			texCoord.x = texCoord.x;
			texCoord.y = (texCoord.y + 2)/8;
		}
	}
	else
	{
		if (TargetPixel.x < Texture1O8)
		{
			//Left border
			//Normalize texture coordinate
			texCoord.x = TargetPixel.x/Texture1O8;
			texCoord.y = ((TargetPixel.y - Texture1O8)/TextureSize.x)%1;
			//Map to texture region
			//Rotate first
			float temp = texCoord.x;
			texCoord.x = texCoord.y;
			texCoord.y = 1 - temp;

			texCoord.x = texCoord.x;
			texCoord.y = (texCoord.y + 4)/8;
		}
		else if (TargetSize.x - TargetPixel.x < Texture1O8)
		{
			//Right border
			//Normalize texture coordinate
			texCoord.x = (TargetPixel.x - TargetSize.x)/Texture1O8 + 1;
			texCoord.y = ((TargetPixel.y - Texture1O8)/TextureSize.x)%1;
			//Map to texture region
			//Rotate first
			float temp = texCoord.x;
			texCoord.x = texCoord.y;
			texCoord.y = 1 - temp;

			texCoord.x = texCoord.x;
			texCoord.y = (texCoord.y + 3)/8;
		}
		else
		{
			//Middle fill
			//Normalize texture coordinate
			texCoord.x = (4*(TargetPixel.x - Texture1O8)/TextureSize.x)%1;
			texCoord.y = (8*(TargetPixel.y - Texture1O8)/texSizeY)%1;
			//Map to texture region
			texCoord.x = texCoord.x/4;
			texCoord.y = (texCoord.y + 5)/8;
		}
	}
	
	if (IsHalfTexture)
	{
		texCoord.y /= 2;
		if (IsBottomHalf)
			texCoord.y += 0.5;
	}
	float4 color = tex2D(ScreenS, texCoord);
    return color * TintColor;    
}

technique Border
{
    pass Pass0
    {
		//VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_2_a BorderPS();
    }
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


float4 NormalPS(float2 texCoord: TEXCOORD0) : COLOR
{
	if (Tiled)
		texCoord = texCoord * TargetSize / TextureSize % 1;
		
	if (IsHalfTexture)
	{
		texCoord.y /= 2;
		if (IsBottomHalf)
			texCoord.y += 0.5;
	}
    return tex2D(ScreenS, texCoord) * TintColor;    
}

technique Normal
{
    pass Pass0
    {
		//VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_2_a NormalPS();
    }
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


float4 NonePS(float2 texCoord: TEXCOORD0) : COLOR
{
    return float4(1, 1, 1, 1) * TintColor;    
}

technique None
{
    pass Pass0
    {
		//VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_2_a NonePS();
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////


float4 EndCapPS(float2 texCoord: TEXCOORD0) : COLOR
{
	float2 targetPixel = texCoord * TargetSize;
	if (targetPixel.x <= TextureSize.x/2)
	{
		//Left edge
		texCoord.x = targetPixel.x/TextureSize.x;
		texCoord.y = texCoord.y/2;
	}
	else if (targetPixel.x >= TargetSize.x - TextureSize.x/2)
	{
		//Right edge
		texCoord.x = (targetPixel.x - TargetSize.x + TextureSize.x/2)/TextureSize.x + 0.5;
		texCoord.y = texCoord.y/2;
	}
	else
	{
		//Middle
		texCoord.x = ((targetPixel.x - TextureSize.x/2)%TextureSize.x)/TextureSize.x;
		texCoord.y = (texCoord.y + 1)/2;
	}
	if (IsHalfTexture)
	{
		texCoord.y /= 2;
		if (IsBottomHalf)
			texCoord.y += 0.5;
	}
	float4 color = tex2D(ScreenS, texCoord);
    return color * TintColor;    
}

technique EndCap
{
    pass Pass0
    {
		//VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_2_a EndCapPS();
    }
}