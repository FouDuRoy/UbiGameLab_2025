using UnityEngine;

public class WoodBloc : MonoBehaviour
{
    [Header("Wood Block Properties")]
    public float resistance = 5f;
    public GameObject replacementPrefab;
    public GameObject woodSfx;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > resistance)
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
            Instantiate(replacementPrefab, transform.position, transform.rotation);

            // D�truit l�objet courant
            gameObject.transform.parent.gameObject.SetActive(false);
            Destroy(gameObject.transform.parent.gameObject);
        }
}