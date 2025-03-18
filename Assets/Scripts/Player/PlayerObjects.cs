using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Federico Barallobres
public class PlayerObjects : MonoBehaviour
{
    [SerializeField] public GameObject player;
    [SerializeField] public Rigidbody cubeRb;
    [SerializeField] public GameObject passiveCube;
    MouvementType moveType;
    protected GridSystem gridSystem;
    public float weight=1;

    void Start()
    {
        gridSystem = FindObjectOfType<GridSystem>();
        weight = 1;
        moveType = this.GetComponent<PlayerMouvement>().moveType;
    }

    public void removeCube(GameObject cube)
    {
        if (moveType == MouvementType.move3dSpring)
        {
            ConfigurableJoint[] joints = cube.GetComponents<ConfigurableJoint>();
            foreach (ConfigurableJoint joint in joints)
            {
                JointDrive xDrive = joint.xDrive;
                xDrive.positionSpring = 0;
                xDrive.positionDamper = 0;
                joint.xDrive = xDrive;

                JointDrive yDrive = joint.yDrive;
                yDrive.positionSpring = 0;
                yDrive.positionDamper = 0;
                joint.yDrive = yDrive;

                JointDrive zDrive = joint.zDrive;
                zDrive.positionSpring = 0;
                zDrive.positionDamper = 0;
                joint.zDrive = zDrive;

                JointDrive angularXDrive = joint.angularXDrive;
                angularXDrive.positionSpring = 0;
                angularXDrive.positionDamper = 0;
                joint.angularXDrive = angularXDrive;

                JointDrive angularYZDrive = joint.angularYZDrive;
                angularYZDrive.positionSpring = 0;
                angularYZDrive.positionDamper = 0;
                joint.angularYZDrive = angularYZDrive;

                JointDrive slerpDrive = joint.slerpDrive;
                slerpDrive.positionSpring = 0;
                slerpDrive.positionDamper = 0;
                Destroy(joint);
                Rigidbody rb2 = passiveCube.GetComponent<Rigidbody>();
                Rigidbody rb =cube.GetComponent<Rigidbody>();
                rb.mass = rb2.mass;
                rb.drag = rb2.drag;
                rb.angularDrag = rb2.angularDrag;
                rb.collisionDetectionMode = rb2.collisionDetectionMode;
                rb.useGravity = rb2.useGravity;
                rb.constraints = rb2.constraints;
            }
        }
        else
        {
            GetComponent<PlayerObjects>().addRigidBody(cube);
        }
        cube.transform.parent = transform.parent;
        cube.layer = 0;
        Destroy(cube.GetComponent<SphereCollider>());
        StartCoroutine(blockNeutral(cube));
    }

    public void addRigidBody(GameObject cube)
    {
        if (!cube.TryGetComponent<Rigidbody>(out _))
        {
            Rigidbody rb = cube.AddComponent<Rigidbody>();
            Rigidbody rb2 = passiveCube.GetComponent<Rigidbody>();

            rb.mass = rb2.mass;
            rb.drag = rb2.drag;
            rb.angularDrag = rb2.angularDrag;
            rb.collisionDetectionMode = rb2.collisionDetectionMode;
            rb.useGravity = rb2.useGravity;
            rb.constraints = rb2.constraints;
        }
    }
    IEnumerator blockNeutral(GameObject block)
    {

        yield return new WaitForSeconds(3f);
        if (block != null)
        {
            block.GetComponent<Bloc>().setOwner("Neutral");
            block.GetComponent<Bloc>().state = BlocState.none;
        }

    }
}
