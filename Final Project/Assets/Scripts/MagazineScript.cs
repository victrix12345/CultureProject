using UnityEngine;

public class MagazineScript : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;
    private int mask;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        mask = LayerMask.GetMask("Map");
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == mask)
        {
            Destroy(rb);
            Destroy(col);
        }
    }
}