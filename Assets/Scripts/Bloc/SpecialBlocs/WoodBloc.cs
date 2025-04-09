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
            // Instancie le nouveau prefab à la même position/rotation
            Instantiate(replacementPrefab, transform.position, transform.rotation);

            // Détruit l’objet courant
            gameObject.transform.parent.gameObject.SetActive(false);
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}