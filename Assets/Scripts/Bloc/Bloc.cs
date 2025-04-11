using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//Federico Barallobres
public class Bloc : MonoBehaviour
{
    [SerializeField] float minimalSpeed = 0.5f;
    [SerializeField] Material magneticMaterial;
    [SerializeField] float maxSpeed = 1000f;
    [SerializeField] private float fadeDuration = 0.5f;
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
    private TrailRenderer trailRend;
    private Gradient trailGradientOrigin;
    private Gradient fadeGradient;
    private void Start()
    {
        winCon = FindObjectOfType<WinCondition>();

        rb = GetComponent<Rigidbody>();
        this.GetComponent<BoxCollider>().contactOffset = contactOffset;
        if(objectToChangeMesh != null){
         meshToChange = objectToChangeMesh.GetComponent<MeshRenderer>();
        }
        trailRend = trail.GetComponent<TrailRenderer>();
        trailGradientOrigin = trailRend.colorGradient;
        fadeGradient  = new Gradient();
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }

    void Update()
    {
        if (state != BlocState.structure && owner != "Neutral" && state != BlocState.projectileAnimation)
        {
            FadeIn();

       

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
        StartCoroutine(UpdateTrailAlpha(0f, 1f));
    }

    public void FadeOut()
    {
        StartCoroutine(UpdateTrailAlpha(1f, 0f));
    }
    IEnumerator UpdateTrailAlpha(float startAlpha, float targetAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            // Update gradient alpha keys
            GradientAlphaKey[] alphaKeys = originalGradient.alphaKeys;
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha *= currentAlpha;
            }

            fadeGradient.SetKeys(
                originalGradient.colorKeys,
                alphaKeys
            );

            trail.colorGradient = fadeGradient;
            yield return null;
        }
    }
}
