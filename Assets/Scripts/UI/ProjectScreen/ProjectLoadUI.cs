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
        yield return Utils.SetupProjectCoroutine( m_LoadScreen, m_ProjectScreen, m_LoadProgressBar , m_LoadProgressBarText );

        Singleton.GetDataManager().LoadProject( _projectFolderPath, Utils.CreateProgressBarProgress( m_LoadProgressBar, m_LoadProgressBarText ), () =>
        {
            Utils.SetupScreenWhileProgress( m_LoadScreen, Singleton.GetMainScreenSystem(), m_SaveProjectButton, m_CancelButton );
        } ); ;
    }




}
