using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinMaxUI : MonoBehaviour
{
    [SerializeField] private RawImage m_HistogramTexture;
    [SerializeField] private TMP_Text m_MinText;
    [SerializeField] private TMP_Text m_MaxText;
    [SerializeField] private TMP_Text m_VariableText;

    private void Start()
    {
        this.DrawUpdate();
    }

    private void RedrawHistogram()
    {
        if( Singleton.GetDataManager().CurrentTimeStepDataAsset != null )
        {

            m_HistogramTexture.material.SetTexture( "_HistTex", Singleton.GetDataManager().CurrentTimeStepDataAsset.HistogramTexture );
        }
    }

    public void DrawUpdate()
    {
        Singleton.GetDataManager().OnDataAssetChanged += asset => RedrawHistogram();

        m_VariableText.text = Singleton.GetDataManager().CurrentVariableName;
        m_MinText.text = System.String.Format( "{0:0.0000}", Singleton.GetDataManager().CurrentVariableMin );
        m_MaxText.text = System.String.Format( "{0:0.0000}", Singleton.GetDataManager().CurrentVariableMax );
    }

    private void Update()
    {
        this.DrawUpdate();
    }

}
