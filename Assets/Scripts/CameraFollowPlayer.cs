using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float zoom = 10f, rotationSpeed = 10f, cameraPitch = 5f;
    //float zoomSpeed = 500f;

    void Start()
    {
        // Sets camera's initial orientation: pitch for up-down, yaw for left-right
        transform.rotation = Quaternion.Euler(cameraPitch, transform.rotation.eulerAngles.y, 0);
        transform.position = player.position - transform.forward * zoom + Vector3.up * 2;
    }

    void Update()
    {
        // Camera Rotation using hotkeys
        if (Mathf.Abs(Input.GetAxis("Rotation")) > 0.1f)
        {
            Quaternion playerRotation = Quaternion.Euler(cameraPitch, transform.rotation.eulerAngles.y + Input.GetAxis("Rotation") * rotationSpeed * 2, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, Time.deltaTime * rotationSpeed);
        }
        // Rotate camera wherever the Player moves
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f) // Add a threshold to avoid small jittery movements
        {
            Quaternion playerRotation = Quaternion.Euler(cameraPitch, transform.rotation.eulerAngles.y + Input.GetAxis("Horizontal") * rotationSpeed, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, playerRotation, Time.deltaTime * rotationSpeed);
        }
        // Camera zoom
        //else
        //{
        //    zoom -= Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
        //    zoom = Mathf.Clamp(zoom, 8f, 15f);
        //}
        transform.position = player.position - transform.forward * zoom + Vector3.up * 2;  // Camera always follows the Player and is a bit up
    }
}