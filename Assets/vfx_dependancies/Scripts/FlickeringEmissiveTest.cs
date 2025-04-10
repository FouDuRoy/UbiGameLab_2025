using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class FlickeringEmissive : MonoBehaviour
{
    [SerializeField] private bool FLicker; // this needs to change if this works 
    [SerializeField][Min(0)] private float FlickerSpeed = 1f;
    [SerializeField] private AnimationCurve BrightnessCurve;

    private Renderer Renderer;
    private List<Material> Materials = new();
    private List<Color> InitialColors = new();

    private const string EMISSIVE_COLOR_NAME = "_EmissionColor";
    private const string EMISSIVE_KEYWORD = "_EMISSION";

    private void Awake()
    {
        Renderer = GetComponent<Renderer>();
        BrightnessCurve.postWrapMode = WrapMode.Loop;

        foreach(Material material in Renderer.materials)
        {
            material.EnableKeyword("_EMISSION");
            //if (Renderer.material.enabledKeywords.Any(item => item.name == EMISSIVE_KEYWORD) && Renderer.material.HasColor(EMISSIVE_COLOR_NAME))
            if (Renderer.material.enabledKeywords.Any(item => item.name == EMISSIVE_KEYWORD) && Renderer.material.HasColor(EMISSIVE_COLOR_NAME))
            {
                Materials.Add(material);
                InitialColors.Add(material.GetColor(EMISSIVE_COLOR_NAME));
            }
            else
            {
                Debug.LogWarning($"{material.name} is not configured to be emissive." + $"so  FlickeringEmissive on {name} cannot animate this material!");
            }
        }
        if (Materials.Count == 0)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (FLicker && Renderer.isVisible)
        {
            float scaledTIme = Time.time * FlickerSpeed;

            for (int i = 0; i < Materials.Count; i++)
            {
                Color color = InitialColors[i];

                float brightness = BrightnessCurve.Evaluate(scaledTIme);
                color = new Color(
                        color.r * Mathf.Pow(2, brightness),
                        color.g * Mathf.Pow(2, brightness),
                        color.b * Mathf.Pow(2, brightness),
                        color.a
                );

                Materials[i].SetColor(EMISSIVE_COLOR_NAME, color);
            }
        }

    }

}
