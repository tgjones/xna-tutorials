//-----------------------------------------------------------------------------
// DrawModel.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 LightViewProj;

float3 LightDirection;
float4 AmbientColor = float4(0.15, 0.15, 0.15, 0);
float DepthBias = 0.001f;

// The "world to shadow atlas partition" transforms.
float4x4 ShadowTransform[4];

// The bounding rectangle (in texture coordinates) for each split.
float4 TileBounds[4];

texture Texture;
sampler TextureSampler = sampler_state
{
    Texture = (Texture);
};

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
};

struct ShadowData
{
    float4 TexCoords_0_1;
    float4 TexCoords_2_3;
    float4 LightSpaceDepth;
};

// Compute shadow parameters (texture coordinates and depth)
// for a given world space position.
ShadowData GetShadowData(float4 worldPosition)
{
    ShadowData result;
    float4 texCoords[4];
    float lightSpaceDepth[4];

    for (int i = 0; i < 4; i++)
    {
        float4 lightSpacePosition = mul(worldPosition, ShadowTransform[i]);
        texCoords[i] = lightSpacePosition / lightSpacePosition.w;
        lightSpaceDepth[i] = texCoords[i].z;
    }

    result.TexCoords_0_1 = float4(texCoords[0].xy, texCoords[1].xy);
    result.TexCoords_2_3 = float4(texCoords[2].xy, texCoords[3].xy);
    result.LightSpaceDepth = float4(
        lightSpaceDepth[0],
        lightSpaceDepth[1], 
        lightSpaceDepth[2], 
        lightSpaceDepth[3]);

    return result;
}

struct ShadowSplitInfo
{
    float2 TexCoords;
    float LightSpaceDepth;
    int SplitIndex;
};

// Find split index, texture coordinates and light space depth.
ShadowSplitInfo GetSplitInfo(ShadowData shadowData)
{
    float2 shadowTexCoords[4] =
    {
        shadowData.TexCoords_0_1.xy,
        shadowData.TexCoords_0_1.zw,
        shadowData.TexCoords_2_3.xy,
        shadowData.TexCoords_2_3.zw
    };

    float lightSpaceDepth[4] =
    {
        shadowData.LightSpaceDepth.x,
        shadowData.LightSpaceDepth.y,
        shadowData.LightSpaceDepth.z,
        shadowData.LightSpaceDepth.w,
    };

    for (int splitIndex = 0; splitIndex < 4; splitIndex++)
    {
        // Check if this is the right split for this pixel.
        if (shadowTexCoords[splitIndex].x >= TileBounds[splitIndex].x &&
            shadowTexCoords[splitIndex].x <= TileBounds[splitIndex].y &&
            shadowTexCoords[splitIndex].y >= TileBounds[splitIndex].z &&
            shadowTexCoords[splitIndex].y <= TileBounds[splitIndex].w)
        {
            ShadowSplitInfo result;
            result.TexCoords = shadowTexCoords[splitIndex];
            result.LightSpaceDepth = lightSpaceDepth[splitIndex];
            result.SplitIndex = splitIndex;
            return result;
        }
    }

    ShadowSplitInfo result;
    result.TexCoords = float2(0, 0);
    result.LightSpaceDepth = 0;
    result.SplitIndex = 4;
    return result;
}

// compute shadow factor: 0 if in shadow, 1 if not
float GetShadowFactor(ShadowData shadowData)
{
    ShadowSplitInfo splitInfo = GetSplitInfo(shadowData);
    float storedDepth = tex2Dlod(ShadowMapSampler, float4(splitInfo.TexCoords, 0, 0)).r;
    return (splitInfo.LightSpaceDepth < storedDepth) ? 1.0f : 0.5f;
}

struct DrawWithShadowMap_VSIn
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct DrawWithShadowMap_VSOut
{
    float4 Position : POSITION0;
    float3 Normal   : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float4 WorldPos : TEXCOORD2;

    ShadowData Shadow : TEXCOORD3;
};

struct CreateShadowMap_VSOut
{
    float4 Position : POSITION;
    float Depth     : TEXCOORD0;
};

// Transforms the model into light space an renders out the depth of the object
CreateShadowMap_VSOut CreateShadowMap_VertexShader(float4 Position: POSITION)
{
    CreateShadowMap_VSOut Out;
    Out.Position = mul(Position, mul(World, LightViewProj)); 
    Out.Depth = Out.Position.z / Out.Position.w;    
    return Out;
}

// Saves the depth value out to the 32bit floating point texture
float4 CreateShadowMap_PixelShader(CreateShadowMap_VSOut input) : COLOR
{ 
    return float4(input.Depth, 0, 0, 0);
}

// Draws the model with shadows
DrawWithShadowMap_VSOut DrawWithShadowMap_VertexShader(DrawWithShadowMap_VSIn input)
{
    DrawWithShadowMap_VSOut Output;

    float4x4 WorldViewProj = mul(mul(World, View), Projection);
    
    // Transform the models verticies and normal
    Output.Position = mul(input.Position, WorldViewProj);
    Output.Normal =  normalize(mul(input.Normal, World));
    Output.TexCoord = input.TexCoord;
    
    // Save the vertices postion in world space
    Output.WorldPos = mul(input.Position, World);

    Output.Shadow = GetShadowData(Output.WorldPos);
    
    return Output;
}

// Determines the depth of the pixel for the model and checks to see 
// if it is in shadow or not
float4 DrawWithShadowMap_PixelShader(DrawWithShadowMap_VSOut input) : COLOR
{ 
    // Color of the model
    float4 diffuseColor = tex2D(TextureSampler, input.TexCoord);
    // Intensity based on the direction of the light
    float diffuseIntensity = saturate(dot(LightDirection, input.Normal));
    // Final diffuse color with ambient color added
    float4 diffuse = diffuseIntensity * diffuseColor + AmbientColor;

    // Find the position of this pixel in light space
    float4 lightingPosition = mul(input.WorldPos, LightViewProj);

    float shadowFactor = GetShadowFactor(input.Shadow);

    diffuse *= shadowFactor;
    
    return diffuse;
}

// Technique for creating the shadow map
technique CreateShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 CreateShadowMap_VertexShader();
        PixelShader = compile ps_2_0 CreateShadowMap_PixelShader();
    }
}

// Technique for drawing with the shadow map
technique DrawWithShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 DrawWithShadowMap_VertexShader();
        PixelShader = compile ps_3_0 DrawWithShadowMap_PixelShader();
    }
}
