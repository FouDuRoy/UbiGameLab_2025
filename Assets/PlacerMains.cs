using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacerMains : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform leftHandTransform;
    public Transform rightHandTransform;
    public Transform golem;
    public GridSystem gridPlayer;
    float maxX=0;
    float minX =0;
    float leftHand = 0;
    float rightHand = 0;
    RaycastHit[] hitsArray = new RaycastHit[500];
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        maxX = MaxDistanceForDirection(golem.right, 30);
        minX = MaxDistanceForDirection(-golem.right, 30);
        if (leftHand != minX)
        {
            leftHand = minX;
            StartCoroutine(moveLeftHand());
        }
        if(rightHand != maxX)
        {
            rightHand = maxX;
            StartCoroutine(moveRightHand());
        }
    }
    IEnumerator moveLeftHand()
    {
        Vector3 destination = golem.position - golem.right * minX+Vector3.up*2;
        float time = 0;
        float t = 0;
        while (t <= 1)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            time += Time.deltaTime;
            t = time / 0.1f;
            destination = golem.position - golem.right * minX+Vector3.up * 2;
            leftHandTransform.position = Vector3.Lerp(leftHandTransform.position, destination, t);
        }
    }
    IEnumerator moveRightHand()
    {
        Vector3 destination = golem.position + golem.right * maxX+Vector3.up * 2;
        float time = 0;
        float t = 0;
        while (t <= 1)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            time += Time.deltaTime;
            t = time / 0.1f;
            destination = golem.position - golem.right * maxX + Vector3.up * 2;
            rightHandTransform.position = Vector3.Lerp(rightHandTransform.position, destination, t);
        }
    }
    public float MaxDistanceForDirection(Vector3 direction, float radius)
    {

        Vector3 golemProjection = new Vector3(golem.position.x, 0, golem.position.z);
        LayerMask mask = LayerMask.GetMask("magneticPlayer1");
        int nbHits = Physics.BoxCastNonAlloc(transform.position, new Vector3(0.01f, 20f, 0.01f), direction, hitsArray, transform.rotation, radius, mask, QueryTriggerInteraction.Ignore);

        for (int i = nbHits - 1; i >= 0; i--)
        {
            Collider hit = hitsArray[i].collider;
            if (hitsArray[i].collider.transform.root == transform)
            {
                return Vector3.Distance(golemProjection, new Vector3(hit.transform.position.x, 0, hit.transform.position.z));
            }
        }
        return 0;
    }
}
