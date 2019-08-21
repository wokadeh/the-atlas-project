using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( MeshRenderer ) )]
public class VolumeRenderer : MonoBehaviour
{
    [SerializeField] private Material m_CartesianMaterial;
    [SerializeField] private Material m_SphericalMaterial;

    [SerializeField] private MeshRenderer m_Groundplane;
    [SerializeField] private MeshRenderer m_EarthSphere;

    [SerializeField] private GameObject m_CartesianAltitudePrefab;

    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private bool m_ShowAltitudeLevels;

    public VolumeRendererMode Mode { get; private set; }

    private MeshRenderer m_Renderer;
    private GameObject m_CartesianAltitudeLevelContainer;
    private List<GameObject> m_Levels;

    private void Start()
    {
        m_Renderer = this.GetComponent<MeshRenderer>();

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
            this.InitMode( true, m_SphericalMaterial, Globals.SPHERICAL_SCALE, Globals.SPHERIAL_ROTATION );

            this.ClearCartesianLevels();
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
        if( _isActive )
        {
            m_Levels = new List<GameObject>();
            for( int i = 0; i < m_DataManager.MetaData.Levels; i++ )
            {
                GameObject cartesianAltitudeLevel = Instantiate(m_CartesianAltitudePrefab, this.transform);
                cartesianAltitudeLevel.name = $"Cartesian_Altitude_Level_" + i;

                float y = ((float)i / m_DataManager.MetaData.Levels) * this.transform.localScale.y;

                cartesianAltitudeLevel.transform.position = new Vector3( 0, y - ( this.transform.localScale.y / 2 ), 0 );

                m_Levels.Add( cartesianAltitudeLevel );
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
