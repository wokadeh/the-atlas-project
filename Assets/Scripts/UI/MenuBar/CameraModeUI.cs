using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraModeUI : MonoBehaviour
{
    [SerializeField] private Toggle m_CameraModeTogglePrefab;
    [SerializeField] private CameraMode m_CameraMode;

    private List<string> m_CameraModeList;

    public void Show( bool _isShown )
    {
        Singleton.GetCameraModeTogglePanel().SetActive( _isShown );
        Singleton.GetDataTypeTogglePanel().SetActive( false );
        Singleton.GetLevelModeTogglePanel().SetActive( false );
    }

    private void Start()
    {
        for( int i = 0; i < this.transform.childCount; i++ )
        {
            Destroy( this.transform.GetChild( i ).gameObject );
        }

        this.m_CameraModeList = new List<string>();

        this.m_CameraModeList.Add( Globals.CAMERA_FIRSTP_TITLE );
        this.m_CameraModeList.Add( Globals.CAMERA_ORBIT_TITLE );

        int index = 0;

        foreach( string cameraModeName in this.m_CameraModeList )
        {

            Toggle toggle = Instantiate( this.m_CameraModeTogglePrefab, this.transform );
            if( index == 0 )
            {
                toggle.isOn = this.name == cameraModeName;
            }

            TMP_Text label = toggle.transform.Find( "Label" ).GetComponent<TMP_Text>();
            label.text = cameraModeName;

            Log.Info( this, "Add " + cameraModeName + " to List" );
            toggle.onValueChanged.AddListener( isOn =>
             {
                 Utils.ToggleItemsOnClick( isOn, toggle, this.transform );

                 if( isOn )
                 {
                     this.m_CameraMode.SetCameraMode( cameraModeName );
                 }
             } );

            index++;
        }
    }
}
