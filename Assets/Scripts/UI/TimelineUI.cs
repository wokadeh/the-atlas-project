using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineUI : MonoBehaviour {
    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private Slider m_TimelineSlider;

    private void Start() {
        m_TimelineSlider.onValueChanged.AddListener(OnTimelineChanged);
    }

    private void OnEnable() {
        if (m_DataManager.CurrentAsset != null) {
            m_TimelineSlider.minValue = 0;
            m_TimelineSlider.maxValue = m_DataManager.DataAssets.Count - 1;
            m_TimelineSlider.wholeNumbers = true;
        }
    }

    private void OnTimelineChanged(float value) {
        DataAsset asset = m_DataManager.DataAssets[(int)value];
        m_DataManager.SetCurrentAsset(asset);
        m_VolumeRenderer.SetData(asset);
    }
}
