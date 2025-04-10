using UnityEngine;

public class ColorFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    public bool flickerEnabled = true;
    public AnimationCurve speedOverTime = AnimationCurve.Constant(0, 1, 5f);
    public float speedMultiplier = 1f;
    public float loopTime = 1f;
    [Range(0.1f, 20f)] public float flickerSpeed = 5f; // Oscillations per second

    public Color flickerColor = Color.black;
    public AnimationCurve flickerPattern = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Renderer Settings")]
    public bool affectChildren = true;

    private Renderer[] targetRenderers;
    private Color[] originalColors;
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
        materialInstances = new Material[targetRenderers.Length];

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] != null && targetRenderers[i].material.HasProperty("_Color"))
            {
                materialInstances[i] = targetRenderers[i].material;
                originalColors[i] = materialInstances[i].color;
            }
        }
    }

    private void ApplyFlickerEffect()
    {
        // Get current speed from animation curve (looping)
        float curveTime = flickerTime % loopTime; // Loop every 5 seconds
        float currentSpeed = speedOverTime.Evaluate(curveTime) * speedMultiplier;

        // Calculate flicker progression (0 to 1)
        //float flickerProgression = Mathf.Sin(Time.time * flickerSpeed * Mathf.PI * 2f) * 0.5f + 0.5f;
        float flickerProgression = Mathf.Sin(flickerTime * currentSpeed * Mathf.PI * 2f) * 0.5f + 0.5f;

        // Apply smoothing and pattern curve
        float curveValue = flickerPattern.Evaluate(flickerProgression);
        float lerpValue = Mathf.SmoothStep(0f, 1f, curveValue);

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] == null || materialInstances[i] == null) continue;

            // Lerp between original color and flicker color
            materialInstances[i].color = Color.Lerp(originalColors[i], flickerColor, lerpValue);
        }
    }

    public void ResetFlickerTimer()
    {
        flickerTime = 0f;
    }

    public void SetFlickerColor(Color newColor)
    {
        flickerColor = newColor;
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