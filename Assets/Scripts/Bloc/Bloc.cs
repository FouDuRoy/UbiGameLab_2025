using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
//Federico Barallobres
public class Bloc : MonoBehaviour
{
    [SerializeField] float minimalSpeed = 0.5f;
    [SerializeField] Material magneticMaterial;
    [SerializeField] float maxSpeed = 1000f;
    [SerializeField] private float fadeDurationLong = 0.2f;
    [SerializeField] private float fadeDurationShort = 0.2f;
    public float contactOffset = 0.1f;
    public float weight;
    public string owner; 
    public BlocState state;
    public Transform ownerTranform;

    public GameObject objectToChangeMesh;
    MeshRenderer meshToChange; 
    private Vector3Int gridPosition;
    public Rigidbody rb;

    public GameObject trail;
    private WinCondition winCon;

    private TrailRenderer trailRendShort;
    private Material trailRendShortMaterial;
    private Gradient trailGradientOriginShort;
    private Gradient fadeGradientShort;

    private TrailRenderer trailRendLong;
    private Material trailRendLongMaterial;
    private Gradient trailGradientOriginLong;
    private Gradient fadeGradientLong;
    private void Start()
    {
        winCon = FindObjectOfType<WinCondition>();

        rb = GetComponent<Rigidbody>();
        this.GetComponent<BoxCollider>().contactOffset = contactOffset;
        if(objectToChangeMesh != null){
         meshToChange = objectToChangeMesh.GetComponent<MeshRenderer>();
        }
     

        trailRendShort = trail.GetComponent<TrailRenderer>();
        trailRendShortMaterial = trailRendShort.material;
        Debug.Log(trailRendShortMaterial);
        trailGradientOriginShort = trailRendShort.colorGradient;
        fadeGradientShort = new Gradient();

        trailRendShortMaterial.SetFloat("_Alpha", 0);
        trailRendShort.time = 0.01f;
        trailRendShort.emitting = false;
        trailRendShort.Clear();

        trailRendLong = trail.transform.Find("second_longer_trail").GetComponent<TrailRenderer>();
        trailRendLongMaterial = trailRendLong.material;
        trailGradientOriginLong = trailRendLong.colorGradient;
        fadeGradientLong = new Gradient();

        trailRendLongMaterial.SetFloat("_Alpha", 0);
        trailRendLong.time = 0.01f;
        trailRendLong.emitting = false;
        trailRendLong.Clear();
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }

    void Update()
    {
        if (state != BlocState.structure && owner != "Neutral" && state != BlocState.projectileAnimation)
        {
            if (state == BlocState.projectile && rb.velocity.magnitude >= winCon.victoryConditionSpeedRange)
            {
                Debug.Log("here");
                FadeIn();
            }
            else
            {
                FadeOut();
            }           
            float speed = rb.velocity.magnitude;

            //If speed is small enough we enable script
            if (speed < minimalSpeed && state == BlocState.magnetic )
            {
                if( this.GetComponent<Feromagnetic>() != null)
                {
                    this.GetComponent<Feromagnetic>().enabled = true;
                  
                }
                if(tag!="explosive"){
                    changeMeshMaterial(magneticMaterial);
                }
                owner = "Neutral";
                ownerTranform = null;
            }
            
            
        }
        

    }
    void FixedUpdate()
    {
        if(rb != null)
        {
            rb.maxLinearVelocity = maxSpeed;

        }
        
    }
    public void setOwner(string owner)
    {
        this.owner = owner;
    }
    public void setState(BlocState state)
    {
        this.state = state;
    }

    public void changeMeshMaterial(Material mat){
        meshToChange.material = mat;
    }
     public void changeMeshMaterialColor(Color color){
        meshToChange.material.color = color;
    }
    public void FadeIn()
    {
        trailRendShort.time = 1f;
        trailRendShort.emitting = true;
        trailRendShort.enabled = true;

        trailRendLong.time = 1f;
        trailRendLong.emitting = true;
        trailRendLong.enabled = true;

        StartCoroutine(UpdateTrailAlpha(0f, 1f,trailGradientOriginLong, fadeGradientLong, trailRendLong,fadeDurationLong, trailRendShortMaterial));
        StartCoroutine(UpdateTrailAlpha(0f, 1f, trailGradientOriginShort, fadeGradientShort, trailRendShort,fadeDurationShort, trailRendShortMaterial));
    }

    public void FadeOut()
    {
        StartCoroutine(UpdateTrailAlpha(1f, 0f, trailGradientOriginLong, fadeGradientLong, trailRendLong,fadeDurationLong, trailRendLongMaterial));
        StartCoroutine(UpdateTrailAlpha(1f, 0f, trailGradientOriginShort, fadeGradientShort, trailRendShort,fadeDurationShort, trailRendShortMaterial));
    }
    IEnumerator UpdateTrailAlpha(float startAlpha, float targetAlpha, Gradient trailGradientOrigin, Gradient fadeGradient,TrailRenderer trailRend,float fadeDuration,Material trailMaterial)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            trailMaterial.SetFloat("_Alpha", currentAlpha);
            // Update gradient alpha keys
            GradientAlphaKey[] alphaKeys = trailGradientOrigin.alphaKeys;
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha = currentAlpha;
            }

            fadeGradient.SetKeys(
                trailGradientOrigin.colorKeys,
                alphaKeys
            );

            trailRend.colorGradient = fadeGradient;
            Debug.Log(trailRend.gameObject + "grad:" + trailRend.colorGradient.alphaKeys[0].alpha);
            yield return null;

            if(targetAlpha == 0)
            {
                trailRend.Clear();
                trailRend.emitting = false;

                trailRend.material.SetFloat("_Alpha", 0);
                trailRend.material.SetFloat("_Cutoff", 0.999f);

                trailRend.enabled = false;
            }
           
        }
    }
}
