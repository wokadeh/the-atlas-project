using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraMode : MonoBehaviour
{
    [SerializeField] private Button m_CameraModeButton;
    [SerializeField] private Camera m_Camera;
    [SerializeField] private Color m_EnabledColor;

    private void Start()
    {
        m_CameraModeButton.onClick.AddListener(() =>
        {

            if (m_Camera.GetComponent<CameraOrbit>().enabled)
            {
                m_Camera.GetComponent<CameraOrbit>().enabled = false;
                m_Camera.GetComponent<CameraFree>().enabled = true;

            }
            else
            {
                m_Camera.GetComponent<CameraOrbit>().enabled = true;
                m_Camera.GetComponent<CameraFree>().enabled = false;
            }
        });
    }
}
