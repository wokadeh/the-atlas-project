using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

public class MinMaxUI : MonoBehaviour
{
    [SerializeField] private RawImage m_HistogramTexture;
    [SerializeField] private TMP_Text m_MinText;
    [SerializeField] private TMP_Text m_MaxText;
    [SerializeField] private TMP_Text m_VariableText;

    private DataManager m_DataManager;

    private void Start()
    {
        m_DataManager = Singleton.GetDataManager();
        m_DataManager.OnDataAssetChanged += asset => RedrawHistogram();

        m_VariableText.text = m_DataManager.CurrentVariableName;
        m_MinText.text = Utils.TryConvertDoubleToDateTimeString( m_DataManager.CurrentVariableMin );
        m_MaxText.text = Utils.TryConvertDoubleToDateTimeString( m_DataManager.CurrentVariableMax );
    }

    private void RedrawHistogram()
    {
        if( m_DataManager.CurrentTimeStepDataAsset != null )
        {

            m_HistogramTexture.material.SetTexture( "_HistTex", m_DataManager.CurrentTimeStepDataAsset.HistogramTexture );
        }
    }

}
