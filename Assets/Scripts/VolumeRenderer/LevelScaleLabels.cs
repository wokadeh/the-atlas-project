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

        for( int i = 0; i < levelList.Length; i++ )
        {
            GameObject label = Instantiate( m_LevelLabelPrefab );

            float linIdx = (float) i / levelList.Length;

            float logIdx = 0;

            // Exception: You cannot log( 0 )
            if( linIdx != 0 )
            {
                logIdx = ( Mathf.Log( linIdx * Globals.MAX_PRESSURE ) / Globals.LOG_MAX_PRESSURE );
            }
      
            label.GetComponent<TextMeshPro>().text = levelList[ i ].ToString();
            label.name = $"Cartesian_Altitude_Level_" + i;
            label.transform.SetParent( this.transform, false );
            label.transform.localScale = new Vector3( 0.005f, 0.01f, 0.1f );

            label.transform.position = new Vector3( 0, -( logIdx * label.transform.localScale.y * 20 ) + 0.1f, 0.41f );
            label.transform.Rotate( 90, 0, 0 );
            m_LabelList.Add( label );
        }
    }
    // Billboard the label so it always faces the camera
    void LateUpdate()
    {
        foreach( GameObject label in m_LabelList )
        {
            label.transform.rotation = m_Camera.transform.rotation;
        }

    }
}
