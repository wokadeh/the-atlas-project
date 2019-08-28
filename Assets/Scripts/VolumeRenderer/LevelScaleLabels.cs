// ****************************** LOCATION ********************************
//
// [UI] CartesianLevelScalePlane (Prefeb Asset) -> attached
//
// ************************************************************************

using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelScaleLabels : MonoBehaviour
{
    [SerializeField] private GameObject m_LevelLabelPrefab;
    private Camera m_Camera;
    private List<GameObject> m_LabelList;

    // Start is called before the first frame update
    void Start()
    {
        m_Camera = Camera.main;
        int[] levelList = Globals.LEVEL_LIST_37();
        m_LabelList = new List<GameObject>();

        for( int i = 0; i < levelList.Length; i++)
        {
            GameObject label = Instantiate( m_LevelLabelPrefab );
            label.GetComponent<TextMeshPro>().text = levelList[i].ToString();
            label.name = $"Cartesian_Altitude_Level_" + i;
            label.transform.SetParent( this.transform, false );
            label.transform.localScale = new Vector3( 0.005f, 0.01f, 0.1f );
            label.transform.position = new Vector3( 0,  - 0.1f + ((levelList.Length - i) / (0.5f* levelList.Length)) * label.transform.localScale.y * 10, 0.375f );
            label.transform.Rotate( 90, 0, 0 );
            m_LabelList.Add(label);
        }
    }
    // Billboard the label so it always faces the camera
    void LateUpdate()
    {
        foreach(GameObject label in m_LabelList)
        {
            label.transform.rotation = m_Camera.transform.rotation;
        }

    }
}
