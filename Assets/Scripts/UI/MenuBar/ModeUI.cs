using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeUI : MonoBehaviour {
    [SerializeField] private Button m_CartesianModeButton;
    [SerializeField] private Image m_CartesianModeButtonImage;
    [SerializeField] private Button m_SphericalModeButton;
    [SerializeField] private Image m_SphericalModeButtonImage;
    [SerializeField] private Color m_EnabledColor;

    private void Start() {
        m_CartesianModeButton.onClick.AddListener(() => {
            if ( Singleton.GetVolumeRenderer().Mode != VolumeRendererMode.Cartesian) {
                m_CartesianModeButtonImage.color = m_EnabledColor;
                m_SphericalModeButtonImage.color = Color.white;

                Singleton.GetVolumeRenderer().SetMode(VolumeRendererMode.Cartesian);
            }
        });

        m_SphericalModeButton.onClick.AddListener(() => {
            if ( Singleton.GetVolumeRenderer().Mode != VolumeRendererMode.Spherical) {
                m_SphericalModeButtonImage.color = m_EnabledColor;
                m_CartesianModeButtonImage.color = Color.white;

                Singleton.GetVolumeRenderer().SetMode(VolumeRendererMode.Spherical);
            }
        });
    }
}
