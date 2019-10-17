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
    public double VarDateFloat;
    public int CurrentIndex;
    public string DateTimeString;

    void Start()
    {
        m_Timestamp3DLabel = this.GetTimestampLabel();


        this.UpdateTimestamp( 0 );
    }

    public void UpdateTimestamp( int _dateIndex )
    {
        m_CurrentData = Singleton.GetDataManager().MetaData;

        if( m_CurrentData != null )
        {
            if( Singleton.GetDataManager().CurrentVariable != null )
            {
                
                CurrentIndex = _dateIndex;

                VarDateFloat = m_CurrentData.Timestamps[ 0 ][ _dateIndex ].DateTimeDouble;
                DateTimeString = Utils.TryConvertDoubleToDateTime( VarDateFloat );

                CurrentDate = Singleton.GetDataManager().CurrentVariable + "_" + DateTimeString;
                this.GetTimestampLabel().text = Singleton.GetDataManager().CurrentVariable + "\n" + DateTimeString;
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
