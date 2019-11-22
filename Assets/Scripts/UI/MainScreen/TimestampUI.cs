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


        //this.UpdateTimestamp( 0 );
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
                    Log.Warn( this, "The timestamp " + _dateIndex.ToString() + " could not be updated: " + e );
                }

                DateTimeString = Utils.TryConvertDoubleToDateTimeString( VarDateFloat );

                CurrentDate = Singleton.GetDataManager().CurrentVariableName + "_" + DateTimeString;
                this.GetTimestampLabel().text = Singleton.GetDataManager().CurrentVariableName + "\n" + DateTimeString;

            }
            else
            {
                Log.Info( this, "No current variable selected, label is empty." );
            }
        }
        else
        {
            Log.Info( this, "No Meta Data found, current data is null and label is empty." );
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
