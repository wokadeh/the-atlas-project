using UnityEngine;

[RequireComponent( typeof( MeshRenderer ) )]
public class VolumeRenderer : MonoBehaviour
{
    [SerializeField] private Material m_CartesianMaterial;
    [SerializeField] private Material m_SphericalMaterial;

    [SerializeField] private MeshRenderer m_Groundplane;
    [SerializeField] private MeshRenderer m_EarthSphere;

    [SerializeField] private GameObject Volume_Renderer_Cartesian_Altitude_Bar;

    public VolumeRendererMode Mode { get; private set; }

    private MeshRenderer m_Renderer;

    private void Start()
    {
        m_Renderer = this.GetComponent<MeshRenderer>();

        Volume_Renderer_Cartesian_Altitude_Bar.transform.localScale = new Vector3( 0.1f, 5F, 0.1f );

        // We always start of in cartesian mode
        this.SetMode( VolumeRendererMode.Cartesian );
    }

    public void SetMode( VolumeRendererMode mode )
    {
        this.Mode = mode;

        if ( mode == VolumeRendererMode.Cartesian )
        {
            m_Groundplane.enabled = true;
            m_EarthSphere.enabled = false;
            m_Renderer.material = m_CartesianMaterial;
            this.transform.localScale = new Vector3( 1f, 0.2f, -0.75f );
            this.transform.rotation = Quaternion.Euler( 180, 0, 0 );
        }
        else if ( mode == VolumeRendererMode.Spherical )
        {
            m_Groundplane.enabled = false;
            m_EarthSphere.enabled = true;
            m_Renderer.material = m_SphericalMaterial;
            this.transform.localScale = new Vector3( 1f, 1f, 1f );
            this.transform.rotation = Quaternion.Euler( -90, 0, 0 );
        }
    }

    public void SetData( TimeStepDataAsset data )
    {
        if( m_Renderer )
        {
            m_Renderer.enabled = true;
        }

        m_CartesianMaterial.SetTexture( "_Data", data.DataTexture );
        m_SphericalMaterial.SetTexture( "_Data", data.DataTexture );
    }

    public void SetTransferFunction( Texture2D transferFunction )
    {
        m_CartesianMaterial.SetTexture( "_TFTex", transferFunction );
        m_SphericalMaterial.SetTexture( "_TFTex", transferFunction );
    }

    public void Disable()
    {
        m_Renderer.enabled = false;
    }
}
