using System.Collections;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
//Federico Barallobres
public class PlayerObjects : MonoBehaviour
{
    [SerializeField] public GameObject player;
    [SerializeField] public Rigidbody cubeRb;
    [SerializeField] public GameObject magneticCube;
    [SerializeField] public GameObject passiveCube;
    [SerializeField] private float magnetTimer = 3f;
    [SerializeField] private float projectionTimer = 0.1f;
    [SerializeField] public GameObject golem;
    Rigidbody magneticCubeRb;
    MouvementType moveType;
    protected GridSystem gridSystem;
    public float weight = 1;
    void Start()
    {
        gridSystem = this.GetComponent<GridSystem>();
        magneticCubeRb = magneticCube.GetComponent<Rigidbody>();
        weight = 1;
    }

    public void removeCube(GameObject cube)
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
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
            joint.projectionMode = JointProjectionMode.None;
            Destroy(joint);
        }
        foreach (var m in gridSystem.grid)
        {
            joints = m.Value.GetComponents<ConfigurableJoint>();
            foreach (ConfigurableJoint joint in joints)
            {
                if (joint.connectedBody.gameObject == cube)
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
                    joint.angularYMotion = ConfigurableJointMotion.Free;
                    joint.angularXMotion = ConfigurableJointMotion.Free;
                    joint.angularZMotion = ConfigurableJointMotion.Free;
                    joint.xMotion = ConfigurableJointMotion.Free;
                    joint.yMotion = ConfigurableJointMotion.Free;
                    joint.zMotion = ConfigurableJointMotion.Free;
                    joint.projectionMode = JointProjectionMode.None;

                    Destroy(joint);


                }

            }
        }
        Rigidbody cubeRigidBody = cube.gameObject.GetComponent<Rigidbody>();
        if (cubeRigidBody == null)
        {
            addRigidBody(cube);

        }
        else
        {
            resetRb(cube);
        }

        cube.transform.parent = transform.parent;
        ConnectMagneticStructure magneticStructure = cube.GetComponent<ConnectMagneticStructure>();
        if (magneticStructure != null)
        {
            cube.transform.Find("Orientation").rotation = cube.transform.rotation;
            StartCoroutine(magenticStructure(cube));
            cube.layer = 0;
        }
        else
        {
            cube.layer = 0;
            Destroy(cube.GetComponent<SphereCollider>());
            StartCoroutine(blockNeutral(cube));

        }
    }
    public void removeCubeProjection(GameObject cube)
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
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
            joint.projectionMode = JointProjectionMode.None;
            Destroy(joint);
        }
        foreach (var m in gridSystem.grid)
        {
            joints = m.Value.GetComponents<ConfigurableJoint>();
            foreach (ConfigurableJoint joint in joints)
            {
                if (joint.connectedBody.gameObject == cube)
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
                    joint.angularYMotion = ConfigurableJointMotion.Free;
                    joint.angularXMotion = ConfigurableJointMotion.Free;
                    joint.angularZMotion = ConfigurableJointMotion.Free;
                    joint.xMotion = ConfigurableJointMotion.Free;
                    joint.yMotion = ConfigurableJointMotion.Free;
                    joint.zMotion = ConfigurableJointMotion.Free;
                    joint.projectionMode = JointProjectionMode.None;

                    Destroy(joint);


                }

            }
        }
        cube.transform.parent = transform.parent;
    }
    public void resetRb(GameObject cube)
    {
        Rigidbody rb2;
        if (cube.tag == "magneticCube")
        {
            rb2 = magneticCube.GetComponent<Rigidbody>();
        }
        else
        {
            rb2 = passiveCube.GetComponent<Rigidbody>();
        }
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        rb.mass = rb2.mass;
        rb.drag = rb2.drag;
        rb.angularDrag = rb2.angularDrag;
        rb.collisionDetectionMode = rb2.collisionDetectionMode;
        rb.useGravity = rb2.useGravity;
        rb.constraints = rb2.constraints;
    }
    public void finishEjection(GameObject cube)
    {
        Destroy(cube.GetComponent<SphereCollider>());
        resetRb(cube);
        StartCoroutine(blockEjection(cube));
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
            cube.GetComponent<Bloc>().rb = rb;
            cube.GetComponent<StoredVelocity>().rb = rb;

        }
    }
    IEnumerator blockNeutral(GameObject block)
    {

        yield return new WaitForSeconds(magnetTimer);
        if (block != null)
        {
            block.GetComponent<Bloc>().state = BlocState.magnetic;
        }

    }
      IEnumerator blockEjection(GameObject block)
    {
        yield return new WaitForSeconds(projectionTimer);
        block.layer = 0;
        yield return new WaitForSeconds(magnetTimer-projectionTimer);
        if (block != null)
        {
            block.GetComponent<Bloc>().state = BlocState.magnetic;
        }

    }
    public void resetEjection(GameObject cube)
    {
        Destroy(cube.GetComponent<SphereCollider>());
        resetRb(cube);
        cube.layer = 0;
        cube.GetComponent<Bloc>().state = BlocState.magnetic; 
    }
    IEnumerator magenticStructure(GameObject block)
    {

        Rigidbody blocRb = block.GetComponent<Rigidbody>();
        do
        {
            yield return null;
        } while (blocRb.velocity.magnitude > 1);
        block.GetComponent<ConnectMagneticStructure>().enabled = true;
        block.GetComponent<Bloc>().state = BlocState.magnetic;
        block.layer = 3;

    }
}
