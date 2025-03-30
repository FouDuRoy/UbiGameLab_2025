using System.Collections;
using UnityEngine;

public class SafeZoneTrigger : MonoBehaviour
{
    [SerializeField] private float groundY = 0.5f;
    [SerializeField] private TutoUI mouvementTutoUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other is BoxCollider)
        {
            WinCondition player = other.GetComponent<WinCondition>(); // Vérifie si l'objet entrant est bien le mainbody du Player

            if (player != null) // Si oui, on le descend de la safezone
            {
                StartCoroutine(LerpToGround(player.gameObject));
                mouvementTutoUI.NextTuto();
            }
        }
    }

    public IEnumerator LerpToGround(GameObject obj)
    {
        Rigidbody rb= obj.GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.constraints &= ~RigidbodyConstraints.FreezePositionY;

        while (obj.transform.position.y > groundY)
        {
            yield return null;
        }

        rb.useGravity = false;
        rb.constraints |= RigidbodyConstraints.FreezePositionY;

        // Assure que la position finale est correcte
        obj.transform.position = new Vector3(obj.transform.position.x, groundY, obj.transform.position.z);
    }
}
