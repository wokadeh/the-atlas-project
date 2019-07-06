using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrepareRenderMode : MonoBehaviour
{
    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private GameObject m_Groundplane;
    [SerializeField] private GameObject m_EarthSphere;


    void LateUpdate()
    {
        MeshRenderer ground = m_Groundplane.GetComponent<MeshRenderer>();
        MeshRenderer sphere = m_EarthSphere.GetComponent<MeshRenderer>();

        if (m_VolumeRenderer.Mode == VolumeRendererMode.Cartesian)
        {
            ground.enabled = true;
            sphere.enabled = false;
            m_VolumeRenderer.GetComponent<MeshRenderer>().material.shader = Shader.Find("Custom/Ray Casting");
            m_VolumeRenderer.transform.localScale = new Vector3(1f, 0.2f, 0.75f);
            m_VolumeRenderer.transform.rotation = Quaternion.identity;
        } else if (m_VolumeRenderer.Mode == VolumeRendererMode.Spherical)
        {
            ground.enabled = false;
            sphere.enabled = true;
            m_VolumeRenderer.GetComponent<MeshRenderer>().material.shader = Shader.Find("Custom/Spherical Ray Casting");
            m_VolumeRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
            m_VolumeRenderer.transform.rotation = Quaternion.Euler(-90, 0, 0);
        }
    }
}
