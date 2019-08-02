using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraMode : MonoBehaviour
{
    //[SerializeField] private Button m_CameraModeButton;
    //[SerializeField] private Image m_CameraModeButtonImage;
    [SerializeField] private Camera m_Camera;
    //[SerializeField] private Color m_EnabledColor;

    public void SetCameraMode(string mode)
    {
        switch(mode)
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

    private void Start()
    {
        //m_CameraModeButton.onClick.AddListener(() =>
        //{

        //    if (m_Camera.GetComponent<CameraOrbit>().enabled)
        //    {
        //        m_Camera.GetComponent<CameraOrbit>().enabled = false;
        //        m_Camera.GetComponent<CameraFree>().enabled = true;

        //        m_CameraModeButtonImage.color = Color.white;
        //    }
        //    else
        //    {
        //        m_Camera.GetComponent<CameraOrbit>().enabled = true;
        //        m_Camera.GetComponent<CameraFree>().enabled = false;

        //        m_CameraModeButtonImage.color = m_EnabledColor;
        //    }
        //});
    }
}
