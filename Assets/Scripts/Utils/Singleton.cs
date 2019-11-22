// ********************************* USE **********************************
//
// This class contains global instances that are initiallized only once!
//
// ************************************************************************

using TMPro;
using UnityEngine;

public class Singleton
{
    private static DataManager m_DataManager;
    private static VolumeRenderer m_VolumeRenderer;

    private static MeshRenderer m_Groundplane;
    private static MeshRenderer m_EarthSphere;

    private static Material m_CartesianMaterial;
    private static Material m_SphericalMaterial;

    private static GameObject m_CartesianLevelScalePlanePrefab;
    private static GameObject m_MainScreenSystem;
    private static GameObject m_MainScreen;
    private static GameObject m_BottomScreen;
    private static GameObject m_ProjectScreen;
    private static GameObject m_TransferfunctionHistogramScreen;
    private static GameObject m_DataTypeTogglePanel;
    private static GameObject m_LevelModeTogglePanel;
    private static GameObject m_CameraModeTogglePanel;
    private static GameObject m_DialogBox;
    private static TextMeshPro m_DialogBoxText;

    public static DataManager GetDataManager()
    {
        if( m_DataManager == null )
        {
            m_DataManager = GameObject.Find( "Volume_Renderer" ).GetComponent<DataManager>();
        }

        return m_DataManager;
    }

    public static VolumeRenderer GetVolumeRenderer()
    {
        if( m_VolumeRenderer == null )
        {
            m_VolumeRenderer = GameObject.Find( "Volume_Renderer" ).GetComponent<VolumeRenderer>();
        }

        return m_VolumeRenderer;
    }

    public static MeshRenderer GetGroundPlane()
    {
        if( m_Groundplane == null )
        {
            m_Groundplane = Singleton.FindInActiveObjectByName( "Groundplane" ).GetComponent<MeshRenderer>();
        }

        return m_Groundplane;
    }

    public static MeshRenderer GetEarthSphere()
    {
        if( m_EarthSphere == null )
        {
            m_EarthSphere = Singleton.FindInActiveObjectByName( "Earth_Octahedron" ).GetComponent<MeshRenderer>();
        }

        return m_EarthSphere;
    }

    public static Material GetCartesianMaterial()
    {
        if( m_CartesianMaterial == null )
        {
            m_CartesianMaterial = ( Material )Resources.Load( Globals.MATERIALS_PATH + "VolumetricCartesianRendering", typeof( Material ) );
            
        }

        return m_CartesianMaterial;
    }

    public static Material GetSphericalMaterial()
    {
        if( m_SphericalMaterial == null )
        {
            m_SphericalMaterial = ( Material )Resources.Load( Globals.MATERIALS_PATH + "VolumetricSphericalRendering", typeof( Material ) );

        }

        return m_SphericalMaterial;
    }

    public static GameObject GetCartesianLevelScalePlanePrefab()
    {
        if( m_CartesianLevelScalePlanePrefab == null )
        {
            m_CartesianLevelScalePlanePrefab = ( GameObject )Resources.Load( Globals.PREFABS_PATH + "CartesianLevelScalePlane", typeof( GameObject ) );
        }

        return m_CartesianLevelScalePlanePrefab;
    }
    public static GameObject GetMainScreenSystem()
    {
        if( m_MainScreenSystem == null )
        {
            m_MainScreenSystem = Singleton.FindInActiveObjectByName( "Main_Screen_System" );
        }

        return m_MainScreenSystem;
    }
    public static GameObject GetMainScreen()
    {
        if( m_MainScreen == null )
        {
            m_MainScreen = Singleton.FindInActiveObjectByName( "Main_Screen" );
        }

        return m_MainScreen;
    }
    public static GameObject GetBottomScreen()
    {
        if( m_BottomScreen == null )
        {
            m_BottomScreen = Singleton.FindInActiveObjectByName( "Bottom_Screen" );
        }

        return m_BottomScreen;
    }
    public static GameObject GetProjectScreen()
    {
        if( m_ProjectScreen == null )
        {
            m_ProjectScreen = Singleton.FindInActiveObjectByName( "Project_Screen" );
        }

        return m_ProjectScreen;
    }
    public static GameObject GetTransferfunctionHistogramScreen()
    {
        if( m_TransferfunctionHistogramScreen == null )
        {
            m_TransferfunctionHistogramScreen = Singleton.FindInActiveObjectByName( "Transfer_Function_Histogram_Panel" );
        }

        return m_TransferfunctionHistogramScreen;
    }

    public static GameObject GetDataTypeTogglePanel()
    {
        if( m_DataTypeTogglePanel == null )
        {
            m_DataTypeTogglePanel = Singleton.FindInActiveObjectByName( "Data_Type_Toggle_Panel" );
        }

        return m_DataTypeTogglePanel;
    }
    public static GameObject GetLevelModeTogglePanel()
    {
        if( m_LevelModeTogglePanel == null )
        {
            m_LevelModeTogglePanel = Singleton.FindInActiveObjectByName( "Level_Mode_Toggle_Panel" );
        }

        return m_LevelModeTogglePanel;
    }

    public static GameObject GetCameraModeTogglePanel()
    {
        if( m_CameraModeTogglePanel == null )
        {
            m_CameraModeTogglePanel = Singleton.FindInActiveObjectByName( "Camera_Mode_Toggle_Panel" );
        }

        return m_CameraModeTogglePanel;
    }
    public static GameObject GetDialogBox()
    {
        if( m_DialogBox == null )
        {
            m_DialogBox = Singleton.FindInActiveObjectByName( "Dialog_Box" );
        }

        return m_DialogBox;
    }

    private static GameObject FindInActiveObjectByName( string name )
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for( int i = 0; i < objs.Length; i++ )
        {
            if( objs[i].hideFlags == HideFlags.None )
            {
                if( objs[i].name == name )
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }
}