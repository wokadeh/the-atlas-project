using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraModeUI : MonoBehaviour
{
    [SerializeField] private Toggle m_CameraModeTogglePrefab;
    [SerializeField] private GameObject m_DataTypeTogglePanel;
    [SerializeField] private CameraMode m_CameraMode;

    private List<string> m_CameraModeList;

    private void Start()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Destroy( this.transform.GetChild( i ).gameObject );
        }

        this.m_CameraModeList = new List<string>();

        this.m_CameraModeList.Add( Globals.CAMERA_ORBIT_TITLE );
        this.m_CameraModeList.Add( Globals.CAMERA_FIRSTP_TITLE );

        int index = 0;

        foreach (string cameraModeName in this.m_CameraModeList)
        {

            Toggle toggle = Instantiate( this.m_CameraModeTogglePrefab, this.transform );
            if (index == 0)
            {
                toggle.isOn = this.name == cameraModeName;
            }

            TMP_Text label = toggle.transform.Find( "Label" ).GetComponent<TMP_Text>();
            label.text = cameraModeName;

            Log.Info( this, "Add " + cameraModeName + " to List" );
            toggle.onValueChanged.AddListener( isOn =>
             {
                 if (isOn)
                 {
                     Log.Info( this, "Toggle is on: " + cameraModeName );
                     this.m_CameraMode.SetCameraMode( cameraModeName );
                     Toggle[] toggles = this.transform.GetComponentsInChildren<Toggle>();
                     if (toggles.Length > 1)
                     {
                         foreach (Toggle t in toggles)
                         {
                             if (t != toggle)
                             {
                                 t.isOn = false;
                             }
                         }
                     }
                 }
             } );

            index++;
        }
    }

    private void Awake()
    {
        this.m_DataTypeTogglePanel.SetActive( false );
    }
}
