// ****************************** LOCATION ********************************
//
// [UI] TimestampLabelPanel -> attached
//
// ************************************************************************

using UnityEngine;
using System;

public class TimestampUI : MonoBehaviour
{
    private TextMesh m_Timestamp3DLabel;

    private IMetaData m_CurrentData;

    public string CurrentDate;
    public double VarDate;
    public int CurrentIndex;
    public DateTime DateTime;

    void Start()
    {
        m_Timestamp3DLabel = this.GetTimestampLabel();

        this.UpdateTimestamp( 0 );
    }

    public void UpdateTimestamp( int _dateIndex )
    {
        Debug.Log( "Updating time index: " + _dateIndex );

        m_CurrentData = Singleton.GetDataManager().MetaData;

        if( this.GetTimestampLabel() != null)
        {
            if( m_CurrentData != null )
            {
                if( Singleton.GetDataManager().CurrentVariable != null )
                {
                    CurrentIndex = _dateIndex;

                    VarDate = m_CurrentData.Timestamps[ 0 ][ _dateIndex ].DateTime;
                    DateTime = DateTime.FromOADate( VarDate - Globals.DATE_FIX_NUMBER );

                    CurrentDate = Singleton.GetDataManager().CurrentVariable + "_" + DateTime.ToString();
                    this.GetTimestampLabel().text = Singleton.GetDataManager().CurrentVariable + "\n" + DateTime.ToString();
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

    private TextMesh GetTimestampLabel()
    {
        if( m_Timestamp3DLabel == null )
        {
            return GetComponent<TextMesh>();
        }
        else
        {
            return m_Timestamp3DLabel;
        }
    }

}
