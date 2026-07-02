using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/Skyward DoF V6 (Wash)")]
public sealed class SkywardDoFVolumeV6 : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    public MinFloatParameter focusDistance = new MinFloatParameter(10f, 0f);
    public MinFloatParameter focusRange = new MinFloatParameter(10f, 0f);

    [Header("Style Aquarelle")]
    public ClampedFloatParameter maxBlurRadius = new ClampedFloatParameter(10f, 1f, 30f);
    public ClampedFloatParameter distortionIntensity = new ClampedFloatParameter(0.002f, 0f, 0.02f);
    public FloatParameter paperScale = new FloatParameter(10f);

    [Header("Distance Wash")]
    public ColorParameter washColor = new ColorParameter(Color.blue, true);
    public ClampedFloatParameter washAlpha = new ClampedFloatParameter(1.0f, 0f, 1f);

    [Header("Teinte Atmosphérique")]
    public ColorParameter dofTintColor = new ColorParameter(Color.white, true);
    public ClampedFloatParameter dofTintIntensity = new ClampedFloatParameter(0.0f, 0f, 1f);

    [Header("Nuages Atmosphériques")]
    public ColorParameter cloudColor = new ColorParameter(Color.white, true);
    public ClampedFloatParameter cloudIntensity = new ClampedFloatParameter(0.05f, 0f, 1f);
    public FloatParameter cloudSpeed = new FloatParameter(0.01f);
    public FloatParameter cloudScale = new FloatParameter(5f);

    [Header("Textures")]
    public TextureParameter noiseTexture = new TextureParameter(null);
    public TextureParameter cloudTexture = new TextureParameter(null);

    Material m_Material;

    public bool IsActive() => m_Material != null && (maxBlurRadius.value > 1f || distortionIntensity.value > 0f || washAlpha.value > 0f) && noiseTexture.value != null;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

    public override void Setup()
    {
        if (Shader.Find("Hidden/PostProcess/SkywardDoF_V6") != null)
            m_Material = new Material(Shader.Find("Hidden/PostProcess/SkywardDoF_V6"));
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null || noiseTexture.value == null || cloudTexture.value == null) return;

        m_Material.SetFloat("_FocusDistance", focusDistance.value);
        m_Material.SetFloat("_FocusRange", focusRange.value);
        m_Material.SetFloat("_MaxBlurRadius", maxBlurRadius.value);       
        m_Material.SetFloat("_DistortionIntensity", distortionIntensity.value);
        m_Material.SetFloat("_PaperScale", paperScale.value);
        
        m_Material.SetColor("_WashColor", washColor.value);
        m_Material.SetFloat("_WashAlpha", washAlpha.value);

        m_Material.SetColor("_DoFTintColor", dofTintColor.value);
        m_Material.SetFloat("_DoFTintIntensity", dofTintIntensity.value);
        m_Material.SetColor("_CloudColor", cloudColor.value);
        m_Material.SetFloat("_CloudIntensity", cloudIntensity.value);
        m_Material.SetFloat("_CloudSpeed", cloudSpeed.value);
        m_Material.SetFloat("_CloudScale", cloudScale.value);
        
        m_Material.SetTexture("_NoiseTex", noiseTexture.value);
        m_Material.SetTexture("_CloudTex", cloudTexture.value);
        m_Material.SetTexture("_MainTex", source);

        HDUtils.DrawFullScreen(cmd, m_Material, destination, shaderPassId: 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}