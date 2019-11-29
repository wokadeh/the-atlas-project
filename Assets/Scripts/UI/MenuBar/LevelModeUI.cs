using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelModeUI : MonoBehaviour
{
    [SerializeField] private Toggle m_LevelModeTogglePrefab;

    private List<string> m_LevelModeList;

    public void Show(bool _isShown)
    {
        Singleton.GetDataTypeTogglePanel().SetActive( false );
        Singleton.GetCameraModeTogglePanel().SetActive( false );
        Singleton.GetLevelModeTogglePanel().SetActive( _isShown );
    }

    private void Start()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Destroy( this.transform.GetChild( i ).gameObject );
        }

        m_LevelModeList = new List<string>();

        int[] levelList37 = Globals.LEVEL_LIST_37();

        m_LevelModeList.Add( "All" );

        foreach(int i in levelList37)
        {
            m_LevelModeList.Add( i.ToString() );
        }

        int index = 0;

        foreach( string levelName in this.m_LevelModeList )
        {

            Toggle toggle = Instantiate( m_LevelModeTogglePrefab, this.transform );
            if( index == 0 )
            {
                toggle.isOn = this.name == levelName;
            }

            TMP_Text label = toggle.transform.Find( "Label" ).GetComponent<TMP_Text>();
            label.text = levelName;

            Log.Info( this, "Add " + levelName + " to List" );
            toggle.onValueChanged.AddListener( isOn =>
             {
                 Utils.ToggleItemsOnClick( isOn, toggle, this.transform );
             } );

            index++;
        }
    }

}
