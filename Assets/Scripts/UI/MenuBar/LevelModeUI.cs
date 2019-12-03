// ****************************** LOCATION ********************************
//
// [UI] Level_Mode_Toggle_Panel -> attached
//
// ************************************************************************

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelModeUI : MonoBehaviour
{
    [SerializeField] private Toggle m_LevelModeTogglePrefab;

    private List<string> m_LevelModeList;

    public void Show( bool _isShown )
    {
        Singleton.GetDataTypeTogglePanel().SetActive( false );
        Singleton.GetCameraModeTogglePanel().SetActive( false );
        Singleton.GetLevelModeTogglePanel().SetActive( _isShown );
    }

    private void DeleteChildrenToggles()
    {
        for( int i = 0; i < this.transform.childCount; i++ )
        {
            Destroy( this.transform.GetChild( i ).gameObject );
        }
    }

    private void SetupLevelList()
    {
        m_LevelModeList = new List<string>();

        int[] levelList37 = Globals.LEVEL_LIST_37();

        m_LevelModeList.Add( "All" );

        foreach( int i in levelList37 )
        {
            m_LevelModeList.Add( i.ToString() );
        }
    }

    private void CreateToggle( string _levelName )
    {
        Toggle toggle = Instantiate( m_LevelModeTogglePrefab, this.transform );
        TMP_Text label = toggle.transform.Find( "Label" ).GetComponent<TMP_Text>();
        label.text = toggle.name = _levelName;
        toggle.isOn = false;

        toggle.onValueChanged.AddListener( isOn =>
        {
            Utils.ToggleItemsOnClick( isOn, toggle, this.transform );
        } );
    }

    private void Start()
    {
        this.DeleteChildrenToggles();

        this.SetupLevelList();

        foreach( string levelName in m_LevelModeList )
        {
            this.CreateToggle( levelName );
        }

        // Select "All" toggle on start
        this.transform.GetComponentsInChildren<Toggle>()[0].isOn = true;
    }

    private void SetLevelInRenderer( int _index )
    {
        Singleton.GetVolumeRenderer().SetLevel( _index );
    }

}
