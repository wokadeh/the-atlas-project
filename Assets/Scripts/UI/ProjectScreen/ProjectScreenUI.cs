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

    private List<Button> m_LeftBarButtonList;

    // Start is called before the first frame update
    private void Start()
    {
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
        if(m_LeftBarButtonList == null || m_LeftBarButtonList.Count == 0)
        {
            m_LeftBarButtonList = new List<Button>();
            for( int i = 0; i < m_LeftBarUI.transform.childCount; i++ )
            {
                if( m_LeftBarUI.transform.GetChild( i ).GetComponent<Button>() != null )
                {
                    m_LeftBarButtonList.Add( m_LeftBarUI.transform.GetChild( i ).GetComponent<Button>() );
                }
            }
            return m_LeftBarButtonList;
        }
        else
        {
            return m_LeftBarButtonList;
        }
    }
}
