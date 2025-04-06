using UnityEngine;

public class DummyShockDetection : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] float victoryConditionSpeedRange = 10f;
    [SerializeField] float victoryConditionSpeedMelee = 15f;
    [SerializeField] GameObject Ennemy;
    [SerializeField] private TutoUI cacTutoUI;
    [SerializeField] private TutoUI shootTutoUI;

    private Rigidbody rb;
    private int nHits = 0;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
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
            string player = this.gameObject.transform.parent.name;

            if (projectileFromOtherPlayer)
            {
                Vector3 hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity;
                Vector3 projectileVelocity = projectileVelocity = hitterVelocity;

                if (projectileVelocity.magnitude > victoryConditionSpeedRange)
                {
                    //print("Range Attack " + this.gameObject.name + " : " + hitter.name + "velocity" + projectileVelocity);
                    DoRotation();
                    nHits++;
                    if (nHits > 2)
                    {
                        shootTutoUI.NextTuto();
                    }
                }
            }
            bool meleeFromOtherPlayer = stateHitter == BlocState.structure && (ownerHitter != ownerHitted);
            if (meleeFromOtherPlayer)
            {
                Vector3 hitterVelocity = hitter.GetComponent<StoredVelocity>().lastTickVelocity;
                Vector3 projectileVelocity = projectileVelocity = hitterVelocity;

                if (projectileVelocity.magnitude > victoryConditionSpeedMelee)
                {
                    //print("Melee attack " + this.gameObject.name + " : " + hitter.name);
                    DoRotation();
                    nHits++;
                    if (nHits > 1)
                    {
                        cacTutoUI.NextTuto();
                    }
                }
            }
        }
    }

    private void DoRotation()
    {
        rb.AddTorque(0, 100f, 0, ForceMode.VelocityChange);
        print("yes");
    }
}
