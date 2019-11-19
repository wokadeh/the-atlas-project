﻿// ********************************* USE **********************************
//
// This class contains global instances that are initiallized only once!
//
// ************************************************************************

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

    public static DataManager GetDataManager()
    {
        if( m_DataManager == null )
        {

            Log.Info( "Singleton", "Anothing happens?" );
            foreach( DataManager c in GameObject.Find( "Volume_Renderer" ).GetComponents<DataManager>())
            {
                Log.Info( "Singleton", c.name );
            }
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
    public static GameObject GetTransferfunctionHistogramScreen()
    {
        if( m_TransferfunctionHistogramScreen == null )
        {
            m_TransferfunctionHistogramScreen = GameObject.Find( "Transfer_Function_Histogram_Panel" );
        }

        return m_TransferfunctionHistogramScreen;
    }

    public static GameObject GetDataTypeTogglePanel()
    {
        if( m_DataTypeTogglePanel == null )
        {
            m_DataTypeTogglePanel = GameObject.Find( "Data_Type_Toggle_Panel" );
        }

        return m_DataTypeTogglePanel;
    }
    public static GameObject GetLevelModeTogglePanel()
    {
        if( m_LevelModeTogglePanel == null )
        {
            m_LevelModeTogglePanel = GameObject.Find( "Level_Mode_Toggle_Panel" );
        }

        return m_LevelModeTogglePanel;
    }
    public static GameObject GetCameraModeTogglePanel()
    {
        if( m_CameraModeTogglePanel == null )
        {
            m_CameraModeTogglePanel = GameObject.Find( "Camera_Mode_Toggle_Panel" );
        }

        return m_CameraModeTogglePanel;
    }
}