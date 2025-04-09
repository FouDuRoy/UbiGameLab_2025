using UnityEngine;

public class WoodBloc : MonoBehaviour
{
    [Header("Wood Block Properties")]
    public float resistance = 5f;
    public GameObject replacementPrefab;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > resistance)
        {
            print("cac");
            // Instancie le nouveau prefab � la m�me position/rotation
            Instantiate(replacementPrefab, transform.position, transform.rotation);

            // D�truit l�objet courant
            gameObject.transform.parent.gameObject.SetActive(false);
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}