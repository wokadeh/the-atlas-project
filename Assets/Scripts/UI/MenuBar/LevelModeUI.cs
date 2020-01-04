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

        foreach( int i in levelList37 )
        {
            m_LevelModeList.Add( i.ToString() );
        }

        m_LevelModeList.Add( Globals.LEVEL_ALL_NAME );
    }

    private void SetupToggles()
    {
        // First create toggles....
        foreach( string levelName in m_LevelModeList )
        {
            this.CreateToggle( levelName );
        }
        // And second create their listeners only after
        for( int i = 0; i < m_LevelModeList.Count; i++ )
        {
            this.AddListenerToggle( this.transform.GetComponentsInChildren<Toggle>()[i] );
        }
    }

    private void CreateToggle( string _levelName )
    {
        Log.Debg( this, "CREATE TOGGLE FOR " + _levelName );
        Toggle toggle = Instantiate( m_LevelModeTogglePrefab, this.transform );
        TMP_Text label = toggle.transform.Find( "Label" ).GetComponent<TMP_Text>();
        label.text = toggle.name = _levelName;
        toggle.isOn = false;
    }

    private void AddListenerToggle( Toggle _toggle )
    {
        _toggle.onValueChanged.AddListener( isOn =>
        {
            Log.Debg( this, "TRIGGERED " + _toggle.name );
            Utils.ToggleItemsOnClick( isOn, _toggle, this.transform );

            this.SetLevelInRenderer( GetToggleIndex( _toggle ) );
        } );
    }

    private int GetToggleIndex( Toggle toggle )
    {
        int toggleIndex = 0;

        if( toggle.name != Globals.LEVEL_ALL_NAME )
        {
            toggleIndex = int.Parse( toggle.name );
        }

        return toggleIndex;
    }

    private void Start()
    {
        this.DeleteChildrenToggles();

        this.SetupLevelList();

        this.SetupToggles();
    }

    private void SetLevelInRenderer( int _index )
    {
        Singleton.GetVolumeRenderer().SetLevel( _index );
    }

}
