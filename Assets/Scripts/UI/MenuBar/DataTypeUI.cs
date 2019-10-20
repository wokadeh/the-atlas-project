using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataTypeUI : MonoBehaviour
{

    [SerializeField] private Toggle m_DataTypeTogglePrefab;
    [SerializeField] private GameObject m_DataTypeTogglePanel;

    private DataManager m_DataManager;
    private bool m_Initialized;

    private void Start()
    {
        m_DataManager = Singleton.GetDataManager();
        this.Initialize();
    }

    public void Show( bool _isShown )
    {
        m_DataTypeTogglePanel.SetActive( _isShown );
    }

    private void OnNewImport()
    {
        // Clean up any old toggles
        for( int i = 0; i < this.transform.childCount; i++ )
        {
            Destroy( this.transform.GetChild( i ).gameObject );
        }



        // Create toggles for all variables
        foreach( IVariable variable in this.m_DataManager.MetaData.Variables )
        {
            string name = variable.Name;
            double min = variable.Min;
            double max = variable.Max;
            Toggle toggle = Instantiate( this.m_DataTypeTogglePrefab, this.transform );
            toggle.isOn = name == this.m_DataManager.CurrentVariableName;

            TMP_Text label = toggle.transform.Find( "Label" ).GetComponent<TMP_Text>();
            label.text = name;

            toggle.onValueChanged.AddListener( isOn =>
            {
                if( isOn )
                {
                    this.m_DataManager.SetCurrentVariable( name, min, max );
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
                        m_DataManager.DisableVolumeRenderer();
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

        this.m_DataManager.OnNewImport += this.OnNewImport;

        if( this.m_DataManager.CurrentTimeStepDataAsset != null )
        {
            this.OnNewImport();
        }

        this.m_Initialized = true;
    }
}
