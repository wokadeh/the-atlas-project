using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataTypeUI : MonoBehaviour
{

    [SerializeField] private Toggle m_DataTypeTogglePrefab;
    [SerializeField] private TimestampUI m_TimestampUI;

    private bool m_Initialized;

    private void Start()
    {
        this.Initialize();
    }

    public void Show( bool _isShown )
    {
        Singleton.GetDataTypeTogglePanel().SetActive( _isShown );
        Singleton.GetCameraModeTogglePanel().SetActive( false );
        Singleton.GetLevelModeTogglePanel().SetActive( false );
    }

    private void OnNewImport()
    {
        // Clean up any old toggles
        for( int i = 0; i < this.transform.childCount; i++ )
        {
            Destroy( this.transform.GetChild( i ).gameObject );
        }
        
        // Create toggles for all variables
        foreach( IVariable variable in Singleton.GetDataManager().MetaData.Variables )
        {
            string name = variable.Name;
            double min = variable.Min;
            double max = variable.Max;
            Toggle toggle = Instantiate( this.m_DataTypeTogglePrefab, this.transform );
            toggle.isOn = name == Singleton.GetDataManager().CurrentVariableName;

            TMP_Text label = toggle.transform.Find( "Label" ).GetComponent<TMP_Text>();
            label.text = name;

            toggle.onValueChanged.AddListener( isOn =>
            {
                if( isOn )
                {
                    Singleton.GetDataManager().SetCurrentVariable( name, min, max );

                    m_TimestampUI.UpdateTimestamp( m_TimestampUI.CurrentIndex );

                    Toggle[] toggles = this.transform.GetComponentsInChildren<Toggle>();

                    if( toggles.Length > 1 )
                    {
                        foreach( Toggle t in toggles )
                        {
                            if( t != toggle )
                            {
                                t.isOn = false;
                            }

                        }
                    }
                }
                else
                {
                    Toggle[] toggles = this.transform.GetComponentsInChildren<Toggle>();
                    bool isOneToggleOn = false;

                    foreach( Toggle t in toggles )
                    {
                        if( t.isOn == true )
                        {
                            isOneToggleOn = true;
                        }
                    }
                    if( isOneToggleOn == false )
                    {
                        Singleton.GetVolumeRenderer().Show( false );
                    }
                }


            } );
        }
    }

    private void Initialize()
    {
        if( this.m_Initialized )
        {
            return;
        }

        Singleton.GetDataManager().OnNewImport += this.OnNewImport;

        if( Singleton.GetDataManager().CurrentTimeStepDataAsset != null )
        {
            this.OnNewImport();
        }

        this.m_Initialized = true;
    }
}
