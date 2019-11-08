using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using TMPro;
using System.IO;

public class ProjectLoadUI : MonoBehaviour
{
    private static readonly ExtensionFilter[] FILE_FILTER = new ExtensionFilter[] {
        new ExtensionFilter("Xml File", "xml")
    };

    [SerializeField] private GameObject m_TransferFunctionUIPanel;
    [SerializeField] private GameObject m_LoadScreen;
    [SerializeField] private Image m_LoadProgressBar;
    [SerializeField] private TMP_Text m_LoadProgressBarText;
    [SerializeField] private Button m_LoadProjectButton;
    [SerializeField] private Button m_SaveProjectButton;
    [SerializeField] private Button m_CancelButton;
    [SerializeField] private GameObject m_ProjectScreen;
    [SerializeField] private GameObject m_ApplicationToptoBottomLayout;


    // Start is called before the first frame update
    void Start()
    {
        m_LoadProjectButton.onClick.AddListener( LoadProject );
    }

    private void LoadProject()
    {
        string[] files = StandaloneFileBrowser.OpenFilePanel("Load project file...", Directory.GetCurrentDirectory() + "\\" + Globals.SAVE_PROJECTS_PATH, FILE_FILTER, false);

        // Only continue, if at least one folder was selected
        if( files.Length > 0 )
        {
            string projectFolderPath = files[0];

            Log.Info( this, "Load from project file " + projectFolderPath );

            // This is a little hackey but works for now
            m_TransferFunctionUIPanel.SetActive( false );

            this.StartCoroutine( LoadProjectCoroutine( projectFolderPath ) );
        }
    }

    private IEnumerator LoadProjectCoroutine( string _projectFolderPath )
    {
        m_LoadProgressBar.fillAmount = 0;
        m_LoadProgressBarText.text = "0 %";

        m_ProjectScreen.SetActive( false );
        m_LoadScreen.SetActive( true );

        // We are waiting for two frames so that unity has enough time to redraw the ui
        // which apparently it needs or otherwise the positions are off...
        yield return null;
        yield return null;

        Singleton.GetDataManager().LoadProject( _projectFolderPath, Utils.CreateProgressBarProgress( m_LoadProgressBar, m_LoadProgressBarText, m_LoadScreen ), () =>
        {
            m_LoadScreen.SetActive( false );
            m_ApplicationToptoBottomLayout.SetActive( true );

            m_SaveProjectButton.interactable = true;
            Singleton.GetVolumeRenderer().gameObject.SetActive( true );
            m_CancelButton.interactable = true;
        } ); ;
    }
}
