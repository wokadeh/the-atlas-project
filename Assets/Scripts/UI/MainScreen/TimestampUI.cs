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
    public uint CurrentIndex;
    public string DateTimeString;

    void Start()
    {
        m_Timestamp3DLabel = this.GetTimestampLabel();


        this.UpdateTimestamp( 0 );
    }

    public void UpdateTimestamp( uint _dateIndex )
    {
        m_CurrentData = Singleton.GetDataManager().MetaData;

        if( m_CurrentData != null )
        {
            if( Singleton.GetDataManager().CurrentVariableName != null )
            {

                CurrentIndex = _dateIndex;

                try
                {
                    VarDateFloat = m_CurrentData.Timestamps[ 0 ][ ( int ) _dateIndex ].DateTimeDouble;

                }
                catch( Exception e )
                {
                    Log.Warn( this, "the timestamp " + _dateIndex.ToString() + " could not be updated: " + e );
                }

                DateTimeString = Utils.TryConvertDoubleToDateTimeString( VarDateFloat );

                CurrentDate = Singleton.GetDataManager().CurrentVariableName + "_" + DateTimeString;
                this.GetTimestampLabel().text = Singleton.GetDataManager().CurrentVariableName + "\n" + DateTimeString;

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
