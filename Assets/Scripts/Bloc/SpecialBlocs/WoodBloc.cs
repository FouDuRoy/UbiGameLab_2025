using UnityEngine;

public class WoodBloc : MonoBehaviour
{
    [Header("Wood Block Properties")]
    public float resistance = 5f;
    public GameObject replacementPrefab;
    public GameObject woodSfx;
    public float explosionForce = 50f;

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.relativeVelocity.magnitude >= resistance)
        {
           

            DestroyAnimation();
        }
    }
   
    

    public void DestroyAnimation()
    {
            // Instancie le son de bois
            GameObject woodSound = Instantiate(woodSfx, transform.position, Quaternion.identity);
            woodSound.transform.parent = null;
            woodSound.GetComponent<AudioSource>().Play();
            Destroy(woodSound, 2f); // D�truit le son apr�s 2 secondes

        // Instancie le nouveau prefab � la m�me position/rotation
             replacementPrefab.transform.position = this.transform.position;
            replacementPrefab.SetActive(true);
            Vector3 centerOfBloc =  gameObject.transform.position;
            replacementPrefab.transform.parent = null;
            foreach(Rigidbody rb in replacementPrefab.GetComponentsInChildren<Rigidbody>())
        {
            rb.AddForce((rb.position-centerOfBloc).normalized*explosionForce,ForceMode.VelocityChange);
        }
           
        
            // D�truit l�objet courant
            gameObject.transform.parent.gameObject.SetActive(false);
            Destroy(gameObject.transform.parent.gameObject);
        }
}