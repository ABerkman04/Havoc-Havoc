using UnityEngine;
using Mirror;

public class CameraFollow : NetworkBehaviour
{
    public float FollowSpeed = 2f;
    public Transform target;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (!isLocalPlayer)
        {
            // Disable the camera if this is not the local player
            if (cam != null) cam.enabled = false;

            // Optional: disable this script completely
            enabled = false;
        }
        else
        {
            // Enable camera and optionally assign the MainCamera tag
            if (cam != null)
            {
                cam.enabled = true;
                cam.tag = "MainCamera"; // only for the local player
            }
        }
    }

    void Update()
    {
        if (!isLocalPlayer || target == null) return;

        // Only update position — keep rotation fixed
        Vector3 newPos = new Vector3(target.position.x, target.position.y, -10f);
        transform.position = Vector3.Slerp(transform.position, newPos, FollowSpeed * Time.deltaTime);

        // Reset camera rotation so it's always fixed (same view)
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

}
