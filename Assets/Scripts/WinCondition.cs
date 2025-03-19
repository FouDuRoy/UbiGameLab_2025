using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCondition : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float victoryConditionSpeedRange = 10f;
    [SerializeField] float victoryConditionSpeedMelee = 10f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitter = collision.collider.gameObject;
        Bloc hitterComponent = hitter.GetComponent<Bloc>();
        if (hitterComponent != null)
        {
            string ownerHitter = hitterComponent.owner;
            string ownerHitted = transform.parent.name;
            BlocState stateHitter = hitterComponent.state;
            bool projectileFromOtherPlayer = stateHitter == BlocState.projectile && (ownerHitter != ownerHitted);

            if (projectileFromOtherPlayer)
            {
                Vector3 projectileVelocity = hitter.GetComponent<Rigidbody>().velocity;

                if (projectileVelocity.magnitude > victoryConditionSpeedRange)
                {
                    this.transform.root.GetComponent<PlayerInfo>().TakeDamage(ownerHitter);
                }
            }
            bool meleeFromOtherPlayer = stateHitter == BlocState.structure && (ownerHitter != ownerHitted);
            if (meleeFromOtherPlayer)
            {
                Vector3 projectileVelocity = hitter.GetComponent<Rigidbody>().velocity;

                if (projectileVelocity.magnitude > victoryConditionSpeedMelee)
                {
                    this.transform.root.GetComponent<PlayerInfo>().TakeDamage(ownerHitter);
                }
            }
        }
    }
}
