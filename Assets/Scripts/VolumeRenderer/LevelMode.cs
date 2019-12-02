using UnityEngine;

public class LevelMode : MonoBehaviour
{
    [SerializeField] private Camera m_Camera;

    public void SetLevelMode( string mode )
    {
        switch( mode )
        {
            case Globals.CAMERA_FIRSTP_TITLE:
                m_Camera.GetComponent<CameraOrbit>().enabled = false;
                m_Camera.GetComponent<CameraFree>().enabled = true;
                break;
            case Globals.CAMERA_ORBIT_TITLE:
                m_Camera.GetComponent<CameraOrbit>().enabled = true;
                m_Camera.GetComponent<CameraFree>().enabled = false;
                break;
        }
    }
}
