using UnityEngine;
using Cursor = UnityEngine.Cursor;

[AddComponentMenu( "Camera-Control/Mouse Orbit with zoom" )]

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
        Vector3 angles = this.transform.eulerAngles;
        this.x = angles.y;
        this.y = angles.x;

        this.rigidbody = this.GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (this.rigidbody != null)
        {
            this.rigidbody.freezeRotation = true;
        }
    }

    void LateUpdate()
    {

        Quaternion rotation = Quaternion.Euler( this.y, this.x, 0 );
        this.distance = Mathf.Clamp( this.distance - Input.GetAxis( "Mouse ScrollWheel" ) * 1, this.distanceMin, this.distanceMax );
        RaycastHit hit;
        if (Physics.Linecast( this.target.position, this.transform.position, out hit ))
        {
            this.distance -= hit.distance;
        }
        Vector3 negDistance = new Vector3( 0.0f, 0.0f, -this.distance );
        Vector3 position = rotation * negDistance + this.target.position;

        this.transform.rotation = rotation;
        this.transform.position = position;

        if (Input.GetMouseButtonDown( this.moveButton ) && !this.orbit)
        {
            this.orbit = true;
            this.m_MousePosition = Input.mousePosition;
        }

        if (this.target && this.orbit)
        {
            this.x += Input.GetAxis( "Mouse X" ) * this.xSpeed * this.distance * 0.02f;
            this.y -= Input.GetAxis( "Mouse Y" ) * this.ySpeed * 0.02f;
        }

        if (Input.GetMouseButtonUp( this.moveButton ))
        {
            this.orbit = false;
            Cursor.SetCursor( null, this.m_MousePosition, CursorMode.Auto );
        }

    }

}

