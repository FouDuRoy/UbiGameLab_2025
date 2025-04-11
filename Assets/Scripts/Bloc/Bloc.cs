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

    GameObject trail;
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

        trail = transform.Find("Trail").gameObject;
        trailRendShort = trail.GetComponent<TrailRenderer>();
        trailRendShort.emitting = false;

        trailRendLong = trail.transform.Find("second_longer_trail").GetComponent<TrailRenderer>();
        trailRendLong.emitting = false;
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

        StartCoroutine(startEmiting(fadeDurationLong, trailRendLong));
        StartCoroutine(startEmiting(fadeDurationShort, trailRendShort));

    }

    public void FadeOut()
    {
        StartCoroutine(stopEmiting(0, trailRendLong));
        StartCoroutine( stopEmiting(0, trailRendShort));
    }
    IEnumerator startEmiting(float time,TrailRenderer trail)
    {
        yield return new WaitForSeconds(time);
        trail.emitting = true;
    }

    IEnumerator stopEmiting(float time, TrailRenderer trail)
    {
        yield return new WaitForSeconds(time);
        trail.emitting = false;
    }
}
