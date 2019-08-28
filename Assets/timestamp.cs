using UnityEngine;
using TMPro;
using System;

public class timestamp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_Text;

    DataManager m_DataManager;

    IMetaData m_CurrentData;

    public string m_CurrentDate;
    public double m_VarDate;
    public int m_CurrentIndex;

    public DateTime m_DateTime;

    void Start()
    {
        m_DataManager = GameObject.Find( "SCRIPTS" ).GetComponent<DataManager>();
        m_Text = this.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        this.UpdateTimestamp( 0 );
    }

    public void UpdateTimestamp( int _dateIndex )
    {
        Debug.Log( "updating time index: " + _dateIndex );

        m_CurrentData = m_DataManager.MetaData;

        if( m_CurrentData != null )
        {
            if( m_DataManager.CurrentVariable != null )
            {
                m_CurrentIndex = _dateIndex;

                m_VarDate = m_CurrentData.Timestamps[ 0 ][ _dateIndex ].DateTime;
                m_DateTime = DateTime.FromOADate( m_VarDate - Globals.DATE_FIX_NUMBER );

               

                m_CurrentDate = m_DataManager.CurrentVariable + "_" + m_DateTime.ToString();

                Log.Info( this, "Current date is: " + m_CurrentDate );

                m_Text.text = m_DataManager.CurrentVariable + "\n" + m_DateTime.ToString();

                Log.Info( this, "Text is: " + m_Text );
            }
            else
            {
                Log.Info( this, "No data to show in label, current variable is null." );
            }
        }
        else
        {
            Log.Info( this, "No data to show in label, current data is null." );
        }
    }

}
