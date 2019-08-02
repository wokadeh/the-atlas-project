using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CameraModeUI : MonoBehaviour {
    [SerializeField] private Toggle m_CameraModeTogglePrefab;
    [SerializeField] private GameObject m_DataTypeTogglePanel;
    [SerializeField] private CameraMode m_CameraMode;

    private List<string> m_CameraModeList;

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        m_CameraModeList = new List<string>();

        m_CameraModeList.Add(Globals.CAMERA_ORBIT_TITLE);
        m_CameraModeList.Add(Globals.CAMERA_FIRSTP_TITLE);

        int index = 0;

        foreach (string cameraModeName in m_CameraModeList)
        {

            Toggle toggle = Instantiate(m_CameraModeTogglePrefab, transform);
            if(index == 0)
            {
                toggle.isOn = name == cameraModeName;
            }            

            TMP_Text label = toggle.transform.Find("Label").GetComponent<TMP_Text>();
            label.text = cameraModeName;

            Log.Info(this, "Add " + cameraModeName + " to List");
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    Log.Info( this, "Toggle is on: " + cameraModeName );
                    m_CameraMode.SetCameraMode(cameraModeName);
                    Toggle[] toggles = transform.GetComponentsInChildren<Toggle>();
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
            });

            index++;
        }
    }

    private void Awake()
    {
        m_DataTypeTogglePanel.SetActive(false);
    }
}
