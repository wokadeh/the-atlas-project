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
        //int[] levelList = Globals.LEVEL_LIST_37();
        //m_LabelList = new List<GameObject>();
        //int i = 0;
        //foreach(int levl in levelList)
        //{
        //    GameObject label = Instantiate( m_LevelLabelPrefab );
        //    label.GetComponent<TextMeshPro>().text = levl.ToString();

        //    label.transform.SetParent( this.transform, true );
        //    label.transform.position = label.transform.position - new Vector3( this.transform.localScale.x / 2F, i / levelList.Length * this.transform.localScale.y, 0 );
        //    label.transform.localScale = new Vector3( 0.1f, 0.1f, 0.1f );
        //    m_LabelList.Add(label);

        //    i++;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
