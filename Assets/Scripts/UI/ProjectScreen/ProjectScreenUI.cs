// ****************************** LOCATION ********************************
//
// [Project_Screen_System] Project_Screen -> attached
//
// ************************************************************************

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProjectScreenUI : MonoBehaviour
{
    [SerializeField] private Button m_ProjectButton;
    [SerializeField] private GameObject m_ProjectScreen;
    [SerializeField] private GameObject m_LeftBarUI;
    [SerializeField] private Button m_MetaDataButton;
    [SerializeField] private Button m_SnapshotButton;

    private List<Button> m_LeftBarButtonList;

    // Start is called before the first frame update
    private void Start()
    {
        Log.Info( this, "its started" );
        m_ProjectButton.onClick.AddListener(ShowProjectScreen);
    }

    private void ShowProjectScreen()
    {
        m_ProjectScreen.SetActive(true);
    }

    private void OnEnable()
    {
        foreach(Button buttonInBar in this.GetButtonList() )
        {
            buttonInBar.interactable = false;
        }
    }

    private void OnDisable()
    {
        foreach( Button buttonInBar in this.GetButtonList() )
        {
            buttonInBar.interactable = true;
        }
    }

    private List<Button> GetButtonList()
    {
        if(m_LeftBarButtonList == null)
        {
            m_LeftBarButtonList = new List<Button>();
            m_LeftBarButtonList.Add( m_MetaDataButton );
            m_LeftBarButtonList.Add( m_SnapshotButton );
            return m_LeftBarButtonList;
        }
        else
        {
            return m_LeftBarButtonList;
        }
    }
}
