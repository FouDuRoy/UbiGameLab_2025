using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for URP

[RequireComponent(typeof(Renderer))]
public class URPColorFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    public bool flickerEnabled = true;
    public AnimationCurve speedOverTime = AnimationCurve.Constant(0, 1, 5f);
    public float speedMultiplier = 1f;
    [Range(0f, 1f)] public float flickerSmoothness = 0.5f;

    [ColorUsage(true, true)]
    public Color hdrFlickerColor = Color.black;

    public AnimationCurve flickerPattern = AnimationCurve.Linear(0, 0, 1, 1);
    public bool affectEmission = true;
    [Min(0)] public float emissionMultiplier = 1f;

    [Header("Renderer Settings")]
    public bool affectChildren = true;

    private Renderer[] targetRenderers;
    private Color[] originalColors;
    private Color[] originalEmissionColors;
    private Material[] materialInstances;
    private float flickerTime;

    private void Start()
    {
        CacheRenderersAndMaterials();
    }

    private void Update()
    {
        if (flickerEnabled)
        {
            flickerTime += Time.deltaTime;
            ApplyFlickerEffect();
        }
    }

    private void CacheRenderersAndMaterials()
    {
        targetRenderers = affectChildren
            ? GetComponentsInChildren<Renderer>()
            : new Renderer[] { GetComponent<Renderer>() };

        originalColors = new Color[targetRenderers.Length];
        originalEmissionColors = new Color[targetRenderers.Length];
        materialInstances = new Material[targetRenderers.Length];

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null)
            {
                materialInstances[i] = targetRenderers[i].material;

                // Cache base color (works the same in URP)
                if (materialInstances[i].HasProperty("_BaseColor"))
                {
                    originalColors[i] = materialInstances[i].GetColor("_BaseColor");
                }
                else if (materialInstances[i].HasProperty("_Color"))
                {
                    originalColors[i] = materialInstances[i].color;
                }

                // Cache emission color - URP uses different property names
                if (affectEmission)
                {
                    if (materialInstances[i].HasProperty("_EmissionColor"))
                    {
                        originalEmissionColors[i] = materialInstances[i].GetColor("_EmissionColor");
                    }
                    else if (materialInstances[i].HasProperty("_EmissiveColor"))
                    {
                        originalEmissionColors[i] = materialInstances[i].GetColor("_EmissiveColor");
                    }
                }
            }
        }
    }

    private void ApplyFlickerEffect()
    {
        float curveTime = flickerTime % 1f;
        float currentSpeed = speedOverTime.Evaluate(curveTime) * speedMultiplier;
        float flickerProgression = Mathf.Sin(flickerTime * currentSpeed * Mathf.PI * 2f) * 0.5f + 0.5f;
        float lerpValue = Mathf.SmoothStep(0f, 1f, flickerPattern.Evaluate(flickerProgression));

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] == null || materialInstances[i] == null) continue;

            // Apply to base color
            if (materialInstances[i].HasProperty("_BaseColor"))
            {
                materialInstances[i].SetColor("_BaseColor",
                    Color.Lerp(originalColors[i], hdrFlickerColor, lerpValue));
            }
            else if (materialInstances[i].HasProperty("_Color"))
            {
                materialInstances[i].color = Color.Lerp(originalColors[i], hdrFlickerColor, lerpValue);
            }

            // Apply to emission - URP specific properties
            if (affectEmission)
            {
                Color targetEmission = hdrFlickerColor * emissionMultiplier;

                if (materialInstances[i].HasProperty("_EmissionColor"))
                {
                    materialInstances[i].SetColor("_EmissionColor",
                        Color.Lerp(originalEmissionColors[i], targetEmission, lerpValue));
                }
                else if (materialInstances[i].HasProperty("_EmissiveColor"))
                {
                    materialInstances[i].SetColor("_EmissiveColor",
                        Color.Lerp(originalEmissionColors[i], targetEmission, lerpValue));
                }
            }
        }
    }

    public void ResetFlickerTimer()
    {
        flickerTime = 0f;
    }

    public void SetFlickerColor(Color newColor)
    {
        hdrFlickerColor = newColor;
    }

    public void SetFlickerEnabled(bool enabled)
    {
        flickerEnabled = enabled;
        if (!enabled)
        {
            ResetToOriginalColors();
        }
    }

    private void ResetToOriginalColors()
    {
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null && materialInstances[i] != null)
            {
                materialInstances[i].color = originalColors[i];
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && targetRenderers != null && !flickerEnabled)
        {
            ResetToOriginalColors();
        }
    }
}