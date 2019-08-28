// ****************************** LOCATION ********************************
//
// [UI] SCRIPTS -> attached
//
// ************************************************************************

using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( MeshRenderer ) )]
public class VolumeRenderer : MonoBehaviour
{
    [SerializeField] private Material m_CartesianMaterial;
    [SerializeField] private Material m_SphericalMaterial;
    [SerializeField] private GameObject m_CartesianLevelScalePlanePrefab;
    
    [SerializeField] private bool m_ShowAltitudeLevels;

    private MeshRenderer m_Groundplane;
    private MeshRenderer m_EarthSphere;
    private DataManager m_DataManager;
    private GameObject m_CartesianLevelScalePlane;

    public VolumeRendererMode Mode { get; private set; }

    private MeshRenderer m_Renderer;
    private GameObject m_CartesianAltitudeLevelContainer;
    private List<GameObject> m_Levels;
    private bool m_IsScaleActive = false;
    private bool m_IsScaleManual = false;

    private void Start()
    {
        m_Renderer = this.GetComponent<MeshRenderer>();
        m_Groundplane = GameObject.Find( "Groundplane" ).GetComponent <MeshRenderer>();
        m_EarthSphere = GameObject.Find( "Earth_Octahedron_Sphere" ).GetComponent <MeshRenderer>();
        m_DataManager = GameObject.Find( "SCRIPTS" ).GetComponent<DataManager>();

        // We always start of in cartesian mode
        this.SetMode( VolumeRendererMode.Cartesian );
    }

    public void SetMode( VolumeRendererMode _mode )
    {
        this.Mode = _mode;

        if( _mode == VolumeRendererMode.Cartesian )
        {
            this.InitMode( true, m_CartesianMaterial, Globals.CARTESIAN_SCALE, Globals.CARTESIAN_ROTATION );

            this.SetAltitudeLevelGridActive( m_ShowAltitudeLevels );
        }
        else if( _mode == VolumeRendererMode.Spherical )
        {
            this.InitMode( false, m_SphericalMaterial, Globals.SPHERICAL_SCALE, Globals.SPHERIAL_ROTATION );

            this.ClearCartesianLevels();

            this.SetAltitudeLevelGridActive( false );
        }
    }

    public void InitMode( bool _isGround, Material _material, Vector3 _scale, Quaternion _rot )
    {
        m_Groundplane.enabled = _isGround;
        m_EarthSphere.enabled = !_isGround;

        m_Renderer.material = _material;
        this.transform.localScale = _scale;
        this.transform.rotation = _rot;
    }

    public void SetData( TimeStepDataAsset _data )
    {
        if( m_Renderer )
        {
            m_Renderer.enabled = true;
        }

        m_CartesianMaterial.SetTexture( "_Data", _data.DataTexture );
        m_SphericalMaterial.SetTexture( "_Data", _data.DataTexture );
    }

    public void SetTransferFunction( Texture2D _transferFunction )
    {
        m_CartesianMaterial.SetTexture( "_TFTex", _transferFunction );
        m_SphericalMaterial.SetTexture( "_TFTex", _transferFunction );
    }

    public void Disable()
    {
        m_Renderer.enabled = false;
    }

    private void SetAltitudeLevelGridActive( bool _isActive )
    {
        m_IsScaleActive = _isActive;
        //GameObject cartesianAltitudeLevel = Instantiate(m_CartesianAltitudeContainerPrefab, this.transform);
        //cartesianAltitudeLevel.name = $"Cartesian_Altitude_Container";

        if(m_IsScaleActive)
        {
            m_CartesianLevelScalePlane = Instantiate(m_CartesianLevelScalePlanePrefab, this.transform);
            m_CartesianLevelScalePlane.name = $"Cartesian_Altitude_Scale_X";
            m_CartesianLevelScalePlane.transform.rotation = Quaternion.Euler( 0, 0, 90 );
        }
        else
        {
            if( m_CartesianLevelScalePlane )
            {
                Destroy( m_CartesianLevelScalePlane );
            }
        }
    }

    private void ClearCartesianLevels()
    {
        if( m_Levels != null )
        {
            foreach( var level in m_Levels )
            {
                Destroy( level.gameObject );
            }
            m_Levels.Clear();
        }
    }
}
