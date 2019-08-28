using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaDataUI : MonoBehaviour
{
    private GameObject m_MetaDataLabel;

    DataManager m_DataManager;


    // Start is called before the first frame update
    void Start()
    {
        m_DataManager = GameObject.Find( "SCRIPTS" ).GetComponent<DataManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
