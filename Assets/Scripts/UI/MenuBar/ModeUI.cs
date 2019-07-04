using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeUI : MonoBehaviour {
    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private Button m_CartesianModeButton;
    [SerializeField] private Image m_CartesianModeButtonImage;
    [SerializeField] private Button m_SphericalModeButton;
    [SerializeField] private Image m_SphericalModeButtonImage;
    [SerializeField] private Color m_EnabledColor;

    private void Start() {
        m_CartesianModeButton.onClick.AddListener(() => {
            if (m_VolumeRenderer.Mode != VolumeRendererMode.Cartesian) {
                m_CartesianModeButtonImage.color = m_EnabledColor;
                m_SphericalModeButtonImage.color = Color.white;

                m_VolumeRenderer.SetMode(VolumeRendererMode.Cartesian);
            }
        });

        m_SphericalModeButton.onClick.AddListener(() => {
            if (m_VolumeRenderer.Mode != VolumeRendererMode.Spherical) {
                m_SphericalModeButtonImage.color = m_EnabledColor;
                m_CartesianModeButtonImage.color = Color.white;

                m_VolumeRenderer.SetMode(VolumeRendererMode.Spherical);
            }
        });
    }
}
