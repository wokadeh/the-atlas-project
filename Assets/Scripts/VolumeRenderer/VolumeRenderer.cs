// ****************************** LOCATION ********************************
//
// [Volume_Renderer] Volume_Renderer -> attached
//
// ************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof( MeshRenderer ) )]
public class VolumeRenderer : MonoBehaviour
{
    [SerializeField] private Button m_LevelScaleButton;
    [SerializeField] private bool m_ShowAltitudeLevels;

    private GameObject m_CartesianLevelScalePlane;
    private GameObject m_CartesianLevelPlane;
    

    public VolumeRendererMode Mode { get; private set; }

    private MeshRenderer m_Renderer;
    private List<GameObject> m_Levels;
    private bool m_IsScaleActive = false;

    private void Start()
    {
        m_Renderer = this.GetComponent<MeshRenderer>();

        // We always start of in cartesian mode
        this.SetMode( VolumeRendererMode.Cartesian );

        m_LevelScaleButton.onClick.AddListener( this.OnLevelScaleChanged );
    }
    private void OnLevelScaleChanged()
    {
        m_ShowAltitudeLevels = !m_ShowAltitudeLevels;

        this.SetAltitudeLevelGridActive( m_ShowAltitudeLevels );
    }

    public void Show( bool _isShown )
    {
        this.gameObject.SetActive( _isShown );
    }
    public void SetMode( VolumeRendererMode _mode )
    {
        this.Mode = _mode;

        if( _mode == VolumeRendererMode.Cartesian )
        {
            this.InitMode( true, Singleton.GetCartesianMaterial(), Globals.CARTESIAN_SCALE, Globals.CARTESIAN_ROTATION );

            this.SetAltitudeLevelGridActive( m_ShowAltitudeLevels );
        }
        else if( _mode == VolumeRendererMode.Spherical )
        {
            this.InitMode( false, Singleton.GetSphericalMaterial(), Globals.SPHERICAL_SCALE, Globals.SPHERIAL_ROTATION );

            this.ClearCartesianLevels();

            this.SetAltitudeLevelGridActive( false );
        }
    }

    public void InitMode( bool _isGround, Material _material, Vector3 _scale, Quaternion _rot )
    {
        Singleton.GetGroundPlane().enabled = _isGround;
        Singleton.GetEarthSphere().enabled = !_isGround;

        m_Renderer.material = _material;
        this.transform.localScale = _scale;
        this.transform.rotation = _rot;
    }

    public void SetData( TimeStepDataAsset _data )
    {
        if( m_Renderer )
        {
            m_Renderer.enabled = true;
            this.Show( true );
        }

        Singleton.GetCartesianMaterial().SetTexture( "_Data", _data.DataTexture );
        Singleton.GetSphericalMaterial().SetTexture( "_Data", _data.DataTexture );
    }

    public void SetTransferFunction( Texture2D _transferFunction )
    {
        Singleton.GetCartesianMaterial().SetTexture( "_TFTex", _transferFunction );
        Singleton.GetSphericalMaterial().SetTexture( "_TFTex", _transferFunction );
    }

    public void Disable()
    {
        if ( m_Renderer )
        {
            m_Renderer.enabled = false;
        }
    }

    private void SetAltitudeLevelGridActive( bool _isActive )
    {
        m_IsScaleActive = _isActive;

        if( m_IsScaleActive )
        {
            m_CartesianLevelScalePlane = Instantiate( Singleton.GetCartesianLevelScalePlanePrefab(), this.transform );
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

    public void SetLevel(int _index)
    {
        if(_index == 0)
        {
            if(m_CartesianLevelPlane != null)
            {
                m_CartesianLevelPlane.SetActive( false );
                m_Renderer.enabled = true;
            }
        }
        else 
        {
            if( m_CartesianLevelPlane == null )
            {
                m_CartesianLevelScalePlane = Instantiate( Singleton.GetCartesianLevelPlanePrefab(), this.transform );
                m_CartesianLevelPlane.SetActive( false );
                m_Renderer.enabled = true;
            }

            m_Renderer.enabled = false;
        }
    }
}
