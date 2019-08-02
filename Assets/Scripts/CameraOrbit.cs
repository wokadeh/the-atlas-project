using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]

public class CameraOrbit : MonoBehaviour
{

    public Transform target;
    public float distance = 2.5f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    private Vector2 m_MousePosition;

    public int moveButton = 1;

    public float distanceMin = .5f;
    public float distanceMax = 5f;

    private Rigidbody rigidbody;

    float x = 0.0f;
    float y = 0.0f;

    bool orbit = false;

    // Use this for initialization
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        rigidbody = GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (rigidbody != null)
        {
            rigidbody.freezeRotation = true;
        }
    }

    void LateUpdate()
    {

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 1, distanceMin, distanceMax);
        RaycastHit hit;
        if (Physics.Linecast(target.position, transform.position, out hit))
        {
            distance -= hit.distance;
        }
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        transform.rotation = rotation;
        transform.position = position;

        if (Input.GetMouseButtonDown(moveButton) && !orbit)
        {
            orbit = true;
            m_MousePosition = Input.mousePosition;
        }

        if (target && orbit)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }

        if (Input.GetMouseButtonUp(moveButton))
        {
            orbit = false;
            Cursor.SetCursor(null, m_MousePosition, CursorMode.Auto);
        }

    }

}

