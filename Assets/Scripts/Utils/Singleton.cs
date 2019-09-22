using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Singleton
{
    private static DataManager m_DataManager;
    private static MeshRenderer m_Groundplane;
    private static MeshRenderer m_EarthSphere;

    public static DataManager GetDataManager()
    {
        if( m_DataManager == null )
        {
            m_DataManager = GameObject.Find( "SCRIPTS" ).GetComponent<DataManager>();
        }

        return m_DataManager;
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
            m_EarthSphere = GameObject.Find( "Earth_Octahedron_Sphere" ).GetComponent<MeshRenderer>();
        }

        return m_EarthSphere;
    }
}