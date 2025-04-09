using UnityEngine;
using System;

public class SimpleColorMultiplier : MonoBehaviour
{
    [SerializeField][Range(0.1f, 5f)] public float multiplyFactor = 1.5f;
    [SerializeField] float colorChangeIntensity = 3f;
    Color chargedColor;
    GridSystem playerGrid;

    void Start()
    {
        Renderer rend = this.GetComponent<Renderer>();
        if (rend != null && rend.material != null)
        {
            Color original = rend.material.color;
            rend.material.color = new Color(
                Mathf.Clamp01(original.r * multiplyFactor),
                Mathf.Clamp01(original.g * multiplyFactor),
                Mathf.Clamp01(original.b * multiplyFactor),
                original.a
            );
            Debug.Log($"Color multiplied by {multiplyFactor}");
        }
        else
        {
            Debug.LogError("Renderer or material missing!");
        }
    }
}