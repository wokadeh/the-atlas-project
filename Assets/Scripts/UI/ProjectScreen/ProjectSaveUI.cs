using SFB;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectSaveUI : MonoBehaviour
{
    [SerializeField] private GameObject m_TransferFunctionUIPanel;
    [SerializeField] private GameObject m_SaveScreen;
    [SerializeField] private Image m_SaveProgressBar;
    [SerializeField] private TMP_Text m_SaveProgressBarText;
    [SerializeField] private Button m_SaveProjectButton;
    [SerializeField] private Button m_SaveProjectAsButton;
    [SerializeField] private GameObject m_ProjectScreen;

    private string m_DefaultProjectDir = Directory.GetCurrentDirectory() + "/" + Globals.SAVE_PROJECTS_PATH;

    // Start is called before the first frame update
    void Start()
    {
        m_SaveProjectAsButton.onClick.AddListener( this.SaveAsProject );
        m_SaveProjectButton.onClick.AddListener( this.SaveProject );
    }

    private void SaveProject()
    {
        this.SaveProject( Singleton.GetDataManager().MetaData.DataName, m_DefaultProjectDir, true );
    }

    private void SaveProject( string _projectFileName, string _projectFolderPath, bool _saveOnlyXml )
    {
        Log.Info( this, " Selected folder is " + _projectFolderPath );

        // This is a little hackey but works for now
        m_TransferFunctionUIPanel.SetActive( false );

        

        this.StartCoroutine( this.SaveProjectCoroutine( _projectFileName, _projectFolderPath, _saveOnlyXml ) );

        m_ProjectScreen.SetActive( false );
    }

    private void SaveAsProject()
    {
        string file = StandaloneFileBrowser.SaveFilePanel( "Save project as...", m_DefaultProjectDir, Singleton.GetDataManager().MetaData.DataName, Globals.XML_FILE_FILTER );

        file = Path.GetFileNameWithoutExtension( file );

        // Only continue, if one folder was selected
        this.SaveProject( file, m_DefaultProjectDir, false );
        Singleton.GetMainScreenSystem().SetActive( true );
    }

    private IEnumerator SaveProjectCoroutine( string _projectFileName, string _projectFolderPath, bool _saveOnlyXml )
    {
        yield return Utils.SetupProjectCoroutine( m_SaveScreen, m_ProjectScreen, m_SaveProgressBar, m_SaveProgressBarText );

        Singleton.GetDataManager().SaveProject( _projectFileName, _projectFolderPath, _saveOnlyXml, Utils.CreateProgressBarProgress( m_SaveProgressBar, m_SaveProgressBarText ), () =>
        {
            m_SaveProjectButton.interactable = true;
            m_SaveProjectAsButton.interactable = false;
            m_SaveScreen.SetActive( false );
        } );
    }
}
