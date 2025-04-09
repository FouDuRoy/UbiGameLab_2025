using UnityEngine;

[ExecuteAlways]
public class DebugDrawColliderz : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        foreach (Collider col in FindObjectsOfType<Collider>())
        {
            if (!col.enabled) continue;

            if (col is BoxCollider box)
            {
                Gizmos.matrix = box.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.matrix = sphere.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (col is CapsuleCollider capsule)
            {
                // Capsule is a bit more complex to draw; handled simply here:
                Gizmos.matrix = capsule.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(capsule.center, capsule.radius); // Approximation
            }
            else if (col is MeshCollider mesh)
            {
                if (mesh.sharedMesh != null)
                {
                    Gizmos.matrix = mesh.transform.localToWorldMatrix;
                    Gizmos.DrawWireMesh(mesh.sharedMesh);
                }
            }
        }

        Gizmos.matrix = Matrix4x4.identity; // Reset
    }
}
