using UnityEngine;

[AddComponentMenu( "Camera-Control/Mouse Orbit with zoom" )]

public class CameraOrbit : MonoBehaviour
{
    [SerializeField] private GameObject m_DataTypeTogglePanel;
    [SerializeField] private GameObject m_CameraModeTogglePanel;

    public Transform m_Target;
    public float m_Distance = 2.5f;
    public float m_XSpeed = 120.0f;
    public float m_YSpeed = 120.0f;

    public int m_MoveButton = 1;

    public float m_DistanceMin = .5f;
    public float m_DistanceMax = 5f;

    private Rigidbody rigidbody;

    float m_XPos = 0.0f;
    float m_YPos = 0.0f;

    bool orbit = false;

    // Use this for initialization
    void Start()
    {
        Vector3 angles = this.transform.eulerAngles;
        m_XPos = angles.y;
        m_YPos = angles.x;

        rigidbody = this.GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if ( rigidbody != null )
        {
            rigidbody.freezeRotation = true;
        }
    }

    void LateUpdate()
    {

        Quaternion rotation = Quaternion.Euler( m_YPos, m_XPos, 0 );
        m_Distance = Mathf.Clamp( m_Distance - Input.GetAxis( "Mouse ScrollWheel" ) * 1, m_DistanceMin, m_DistanceMax );
        RaycastHit hit;
        if ( Physics.Linecast( m_Target.position, this.transform.position, out hit ) )
        {
            m_Distance -= hit.distance;
        }
        Vector3 negDistance = new Vector3( 0.0f, 0.0f, -m_Distance );
        Vector3 position = rotation * negDistance + m_Target.position;

        this.transform.rotation = rotation;
        this.transform.position = position;

        if ( Input.GetMouseButtonDown( m_MoveButton ) && !orbit )
        {
            orbit = true;
        }

        if ( m_Target && orbit )
        {
            m_XPos += Input.GetAxis( "Mouse X" ) * m_XSpeed * m_Distance * 0.02f;
            m_YPos -= Input.GetAxis( "Mouse Y" ) * m_YSpeed * 0.02f;
        }

        if ( Input.GetMouseButtonUp( m_MoveButton ) )
        {
            orbit = false;

            m_DataTypeTogglePanel.SetActive( false );
            m_CameraModeTogglePanel.SetActive( false );
        }

    }

}

