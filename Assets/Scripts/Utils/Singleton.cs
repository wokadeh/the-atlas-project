using UnityEngine;

public class Singleton
{
    private static DataManager m_DataManager;
    private static MeshRenderer m_Groundplane;
    private static MeshRenderer m_EarthSphere;
    private static VolumeRenderer m_VolumeRenderer;
    private static Material m_CartesianMaterial;
    private static Material m_SphericalMaterial;
    private static GameObject m_CartesianLevelScalePlanePrefab;
    private static GameObject m_MainScreenSystem;
    private static GameObject m_MainScreen;
    private static GameObject m_BottomScreen;
    private static GameObject m_ProjectScreen;

    public static DataManager GetDataManager()
    {
        if( m_DataManager == null )
        {
            m_DataManager = GameObject.Find( "SCRIPTS" ).GetComponent<DataManager>();
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
            m_Groundplane = GameObject.Find( "Groundplane" ).GetComponent<MeshRenderer>();
        }

        return m_Groundplane;
    }

    public static MeshRenderer GetEarthSphere()
    {
        if( m_EarthSphere == null )
        {
            m_EarthSphere = GameObject.Find( "Earth_Octahedron" ).GetComponent<MeshRenderer>();
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
            m_MainScreenSystem = GameObject.Find( "Main_Screen_System" );
        }

        return m_MainScreenSystem;
    }
    public static GameObject GetMainScreen()
    {
        if( m_MainScreen == null )
        {
            m_MainScreen = GameObject.Find( "Main_Screen" );
        }

        return m_MainScreen;
    }
    public static GameObject GetBottomScreen()
    {
        if( m_BottomScreen == null )
        {
            m_BottomScreen = GameObject.Find( "Bottom_Screen" );
        }

        return m_BottomScreen;
    }
    public static GameObject GetProjectScreen()
    {
        if( m_ProjectScreen == null )
        {
            m_ProjectScreen = GameObject.Find( "Project_Screen" );
        }

        return m_ProjectScreen;
    }
}