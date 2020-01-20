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

    public VolumeRendererMode Mode { get; private set; }

    private MeshRenderer m_Renderer;
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
 }
