using UnityEngine;
using System.Collections;

public class AutoCleanupBlock : MonoBehaviour
{
    public float delayBeforeCleanup = 1f;
    public Vector3 escapeDirection = Vector3.down * 100f; // ou Vector3.forward * 100 si tu veux qu’il traverse horizontalement
    public float moveSpeed = 5f;
    private bool isEscaping = false;

    void Start()
    {
        StartCoroutine(CleanupAfterDelay());
    }

    IEnumerator CleanupAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeCleanup);

        // Supprime tous les Rigidbodies et Colliders dans les enfants
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            print(rb.name);
            Destroy(rb);
        }

        foreach (var col in GetComponentsInChildren<Collider>())
        {
            Destroy(col);
        }

        // Commence à le faire "s'échapper"
        isEscaping = true;
    }

    void Update()
    {
        if (isEscaping)
        {
            transform.position += escapeDirection.normalized * moveSpeed * Time.deltaTime;
            Destroy(gameObject, 2f);
        }
    }
}
