Shader "Hidden/PostProcess/SkywardDoF_V6"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

    TEXTURE2D_X(_MainTex); 
    TEXTURE2D(_NoiseTex);  
    SAMPLER(sampler_NoiseTex);
    TEXTURE2D(_CloudTex);  
    SAMPLER(sampler_CloudTex);

    float _FocusDistance;
    float _FocusRange;
    float _MaxBlurRadius; 
    float _DistortionIntensity; 
    float _PaperScale;     
    
    // Nuages
    float3 _CloudColor;
    float _CloudIntensity;
    float _CloudSpeed;
    float _CloudScale;

    // Teinte (Optionnelle)
    float3 _DoFTintColor;
    float _DoFTintIntensity;

    // NOUVEAU V6 : Distance Wash (Couleur nette du fond)
    float3 _WashColor;
    float _WashAlpha;

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    float4 Frag(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = input.texcoord;
        float4 originalColor = LOAD_TEXTURE2D_X(_MainTex, uv * _ScreenSize.xy);

        float depth = LoadCameraDepth(uv * _ScreenSize.xy);
        float linearDepth = LinearEyeDepth(depth, _ZBufferParams);

        // 1. Calcul du masque DoF doux
        float dofFactor = smoothstep(_FocusDistance, _FocusDistance + _FocusRange, linearDepth);

        // Optimisation
        if (dofFactor <= 0.001)
            return originalColor;

        // 2. Bruit de papier
        float2 paperUV = uv * _PaperScale;
        float paperNoise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, paperUV).r;

        // 3. Nuages
        float2 animatedCloudUV = uv * _CloudScale;
        animatedCloudUV += float2(_Time.y, _Time.y) * _CloudSpeed + (paperNoise - 0.5) * 0.02;
        float cloudSample = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, animatedCloudUV).r;
        float cloudFinalAlpha = cloudSample * _CloudIntensity * dofFactor;

        // 4. Convolution (Flou aquarelle)
        float3 stylizedColor = float3(0, 0, 0);
        float totalWeight = 0.0;
        
        float currentBlurRadius = _MaxBlurRadius * dofFactor;
        float currentDistortion = _DistortionIntensity * dofFactor;
        float2 baseDistortedUV = uv + ((paperNoise - 0.5) * currentDistortion);

        float2 samples[9] = { float2(0, 0), float2(1, 0), float2(-1, 0), float2(0, 1), float2(0, -1), float2(1, 1), float2(-1, 1), float2(1, -1), float2(-1, -1) };
        float weights[9] = { 4.0, 2.0, 2.0, 2.0, 2.0, 1.0, 1.0, 1.0, 1.0 };

        for (int i = 0; i < 9; i++)
        {
            float2 offsetUV = samples[i] / _ScreenSize.xy * currentBlurRadius;
            float2 finalUV = baseDistortedUV + offsetUV;
            stylizedColor += LOAD_TEXTURE2D_X(_MainTex, finalUV * _ScreenSize.xy).rgb * weights[i];
            totalWeight += weights[i];
        }
        stylizedColor /= totalWeight;

        // 5. Application de la Teinte (Optionnelle, V5)
        float3 tintedWash = stylizedColor * _DoFTintColor;
        stylizedColor = lerp(stylizedColor, tintedWash, _DoFTintIntensity * dofFactor);

        // Application du Distance Wash (Brouillard de couleur nette)
        // C'est ici que l'arrière-plan moyen se transforme en une couleur unie
        float3 colorWithWash = lerp(stylizedColor, _WashColor, _WashAlpha * dofFactor);

        // 7. Mélange final avec les Nuages et le premier plan
        // Les nuages dérivent *au-dessus* de cette couleur unie
        float3 colorWithClouds = lerp(colorWithWash, _CloudColor, cloudFinalAlpha);
        
        return lerp(originalColor, float4(colorWithClouds, 1.0), dofFactor);
    }
    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "SkywardDoFPassV6"
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
            #pragma fragment Frag
            #pragma vertex Vert
            ENDHLSL
        }
    }
}