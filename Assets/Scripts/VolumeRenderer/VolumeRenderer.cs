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
            this.InitMode( true, Singleton.GetCartesian3DMaterial(), Globals.CARTESIAN_SCALE, Globals.CARTESIAN_ROTATION );

            this.SetAltitudeLevelGridActive( m_ShowAltitudeLevels );
        }
        else if( _mode == VolumeRendererMode.Spherical )
        {
            this.InitMode( false, Singleton.GetSpherical3DMaterial(), Globals.SPHERICAL_SCALE, Globals.SPHERIAL_ROTATION );

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

    public void SetTexture3D( TimeStepDataAsset _timeStepDataAsset )
    {
        Log.Info( this, "Set 3D texture" );
        if( m_Renderer )
        {
            m_Renderer.enabled = true;
            this.Show( true );
        }

        Singleton.GetCartesian3DMaterial().SetTexture( "_Data", _timeStepDataAsset.DataTexture3D );
        Singleton.GetSpherical3DMaterial().SetTexture( "_Data", _timeStepDataAsset.DataTexture3D );
    }

    //public void SetTexture2D( TimeStepDataAsset _timeStepDataAsset )
    //{
    //    Log.Info( this, "Set 2D texture" );
    //    if( m_Renderer )
    //    {
    //        m_Renderer.enabled = false;
    //        this.Show( false );
    //    }

    //    this.SetCartesianLevelPlaneActive( _timeStepDataAsset.DataTexture2D );
    //    //this.SetSphericalLevelOctahedronActive( _timeStepDataAsset.DataTexture2D );
    //}

    public void SetTransferFunction( Texture2D _transferFunction )
    {
        Singleton.GetCartesian3DMaterial().SetTexture( "_TFTex", _transferFunction );
        Singleton.GetSpherical3DMaterial().SetTexture( "_TFTex", _transferFunction );
       // Singleton.GetCartesian2DMaterial().SetTexture( "_TFTex", _transferFunction );
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

    //private void SetCartesianLevelPlaneActive( Texture2D _tex2D )
    //{
    //    if( m_CartesianLevelPlane )
    //    {
    //        Log.Debg( this, "Delete cartesian level plane!" );
    //        Destroy( m_CartesianLevelPlane );
    //    }

    //    Log.Debg( this, "Set cartesian plane active!" );
    //    if( _tex2D != null )
    //    { 
    //        Log.Debg( this, "Cartesian plane texture is not null, so initiate plane!" );
    //        m_CartesianLevelPlane = Instantiate( Singleton.GetCartesianLevelPlanePrefab(), this.transform );
    //        m_CartesianLevelPlane.gameObject.GetComponent<Renderer>().enabled = true;
    //        m_CartesianLevelPlane.gameObject.GetComponent<Renderer>().material = Singleton.GetCartesian2DMaterial();
    //        m_CartesianLevelPlane.name = $"Cartesian_Level_Plane_Prefab";
    //        m_CartesianLevelPlane.transform.rotation = Quaternion.Euler( 180, 0, 0 );
    //        Singleton.GetCartesian2DMaterial().SetTexture( "_Data", _tex2D );
    //    }
    //    else
    //    {
    //        Log.Debg( this, "Cartesian plane texture is NULL, so drop plane!" );
    //    }
    //}

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
