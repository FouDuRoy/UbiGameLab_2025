using UnityEngine;

public class WinCondition : MonoBehaviour
{
    // Start is called before the first frame update
    public float victoryConditionSpeedRange = 10f;
    public float victoryConditionSpeedMelee = 15f;
    [SerializeField] public GameObject Ennemy;
    [SerializeField] float rangeDamageFactor = 1f;
    [SerializeField] float meleeDamageFactor= 1f;

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
           hitterComponent.ownerTranform != null && hitterComponent.ownerTranform.tag != "magneticCube"
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
                    this.transform.root.GetComponent<PlayerInfo>().TakeDamage(Ennemy.name, projectileVelocity*rangeDamageFactor,false);
                }
            }
            bool meleeFromOtherPlayer = stateHitter == BlocState.structure && (ownerHitter != ownerHitted);
            if (meleeFromOtherPlayer)
            {
                Vector3 hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity;
                Vector3 projectileVelocity  = hitterVelocity;
                if (projectileVelocity.magnitude > victoryConditionSpeedMelee)
                {
                    this.transform.root.GetComponent<PlayerInfo>().TakeDamage(Ennemy.name, projectileVelocity*meleeDamageFactor,true);
                }
            }
        }
    }
}
