using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeroDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDrawGizmos() {
        SphereCollider influence = this.gameObject.GetComponent<SphereCollider>();
        if (influence != null) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, influence.radius * 0.4f);
        } else {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, this.gameObject.GetComponent<Feromagnetic>().getPassiveRadius());
        }
    }
}
