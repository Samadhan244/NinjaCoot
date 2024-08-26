using UnityEngine;

public class Lava : MonoBehaviour
{
    Vector3 startingPosition;
    Rigidbody rb;
    [SerializeField] float fallInEvery;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        InvokeRepeating(nameof(Falling), Random.Range(0f, 3f), fallInEvery);
    }

    void Falling()
    {
        rb.linearVelocity = Vector3.zero;
        transform.position = startingPosition;
    }
}
