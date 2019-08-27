using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ********** LOCATION ************
//
// [UI] CartesianLevelScalePlane (Prefeb Asset) -> attached
//
// ********************************

public class LevelScaleLabels : MonoBehaviour
{
    [SerializeField] private GameObject m_LevelLabelPrefab;
    private List<GameObject> m_LabelList;

    // Start is called before the first frame update
    void Start()
    {
        int[] levelList = Globals.LEVEL_LIST_37();
        m_LabelList = new List<GameObject>();
        int i = 0;
        Debug.Log( "level list length " + levelList.Length   );

        foreach(int levl in levelList)
        {
            GameObject label = Instantiate( m_LevelLabelPrefab );
            label.GetComponent<TextMeshPro>().text = levl.ToString();
            label.name = $"Cartesian_Altitude_Level_" + i;
            label.transform.SetParent( this.transform, false );
            label.transform.localScale = new Vector3( 0.01f, 0.01f, 0.1f );
            label.transform.position = new Vector3( 0,  - 0.1f + ((float) i / (0.5f* levelList.Length)) * label.transform.localScale.y * 10, label.transform.localScale.z * 3.5F );
            label.transform.Rotate( 90, 0, 0 );
            m_LabelList.Add(label);

            Debug.Log( "Position: " + i + " is " + label.transform.position );
            Debug.Log( "Local Scale: " + label.transform.localScale );
            Debug.Log( "Local Rotation: " + label.transform.rotation );
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
