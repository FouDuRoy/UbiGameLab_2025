using UnityEngine;

public class WinCondition : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float victoryConditionSpeedRange = 10f;
    [SerializeField] float victoryConditionSpeedMelee = 15f;
    [SerializeField] GameObject Ennemy;

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

        if (hitterComponent != null && 
           hitterComponent.ownerTranform != null && hitterComponent.ownerTranform.tag != "magneticStructure"
            )
        {
            string ownerHitter = hitterComponent.owner;
            string ownerHitted = transform.parent.name;
            BlocState stateHitter = hitterComponent.state;
            bool projectileFromOtherPlayer = stateHitter == BlocState.projectile && (ownerHitter != ownerHitted);
            string player = this.gameObject.transform.parent.name;

            if (projectileFromOtherPlayer)
            {

                Vector3 hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity;
                Vector3 projectileVelocity  = hitterVelocity;
                if (projectileVelocity.magnitude > victoryConditionSpeedRange)
                {
                    print("Range Attack " + this.gameObject.name + " : " + hitter.name + "velocity" + projectileVelocity);
                    this.transform.root.GetComponent<PlayerInfo>().TakeDamage(Ennemy.name, projectileVelocity,false);
                }
            }
            bool meleeFromOtherPlayer = stateHitter == BlocState.structure && (ownerHitter != ownerHitted);
            if (meleeFromOtherPlayer)
            {
                Vector3 hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity;
                Vector3 projectileVelocity  = hitterVelocity;

                if (projectileVelocity.magnitude > victoryConditionSpeedMelee)
                {
                    print("Melee attack " + this.gameObject.name + " : " + hitter.name);
                    this.transform.root.GetComponent<PlayerInfo>().TakeDamage(Ennemy.name, projectileVelocity,true);
                }
            }
        }
    }
}
