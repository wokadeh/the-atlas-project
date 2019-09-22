// ****************************** LOCATION ********************************
//
// [UI] TimestampLabelPanel -> attached
//
// ************************************************************************

using UnityEngine;
using TMPro;
using System;

public class TimestampUI : MonoBehaviour
{
    //private TextMeshProUGUI m_Text;
    //[SerializeField] private TextMesh m_Timestamp3DLabel;
    private DataManager m_DataManager;
    private TextMesh m_Timestamp3DLabel;

    private IMetaData m_CurrentData;

    public string CurrentDate;
    public double VarDate;
    public int CurrentIndex;
    public DateTime DateTime;

    void Start()
    {
        m_DataManager = this.GetDataManager();
        //m_Text = this.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        m_Timestamp3DLabel = this.GetTimestampLabel();

        this.UpdateTimestamp( 0 );
    }

    public void UpdateTimestamp( int _dateIndex )
    {
        Debug.Log( "Updating time index: " + _dateIndex );

        m_CurrentData = this.GetDataManager().MetaData;

        if( this.GetTimestampLabel() != null)
        {
            if( m_CurrentData != null )
            {
                if( this.GetDataManager().CurrentVariable != null )
                {
                    CurrentIndex = _dateIndex;

                    VarDate = m_CurrentData.Timestamps[ 0 ][ _dateIndex ].DateTime;
                    DateTime = DateTime.FromOADate( VarDate - Globals.DATE_FIX_NUMBER );

                    CurrentDate = this.GetDataManager().CurrentVariable + "_" + DateTime.ToString();
                    this.GetTimestampLabel().text = this.GetDataManager().CurrentVariable + "\n" + DateTime.ToString();
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
        else
        {
            Log.Error( this, "Time label is null" );
        }
    }

    private DataManager GetDataManager()
    {
        if(m_DataManager == null)
        {
            return GameObject.Find( "SCRIPTS" ).GetComponent<DataManager>();
        }
        else
        {
            return m_DataManager;
        }
    }

    private TextMesh GetTimestampLabel()
    {
        if( m_Timestamp3DLabel == null )
        {
            foreach(Component cp in transform)
            {
                Log.Info( this, cp.ToString() );
            }

            return GetComponent<TextMesh>();
        }
        else
        {
            return m_Timestamp3DLabel;
        }
    }

}
