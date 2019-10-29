using SFB;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectSaveUI : MonoBehaviour
{
    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private GameObject m_TransferFunctionUIPanel;
    [SerializeField] private GameObject m_SaveScreen;
    [SerializeField] private Image m_SaveProgressBar;
    [SerializeField] private TMP_Text m_SaveProgressBarText;
    [SerializeField] private Button m_SaveProjectButton;
    [SerializeField] private Button m_SaveProjectAsButton;
    [SerializeField] private GameObject m_ProjectScreen;
    [SerializeField] private GameObject m_ApplicationToptoBottomLayout;

    private string m_DefaultProjectDir = Directory.GetCurrentDirectory() + "/" + Globals.SAVE_PROJECTS_PATH;

    // Start is called before the first frame update
    void Start()
    {
        m_SaveProjectAsButton.onClick.AddListener( this.SaveAsProject );
        m_SaveProjectButton.onClick.AddListener( this.SaveProject );
    }

    private void SaveProject()
    {
        this.SaveProject( m_DataManager.MetaData.DataName, m_DefaultProjectDir, true );
    }

    private void SaveProject( string _projectFileName, string _projectFolderPath, bool _saveOnlyXml )
    {
        Log.Info( this, " Selected folder is " + _projectFolderPath );

        // This is a little hackey but works for now
        m_TransferFunctionUIPanel.SetActive( false );
        m_ProjectScreen.SetActive( false );
        m_ApplicationToptoBottomLayout.SetActive( true );

        this.StartCoroutine( this.SaveProjectCoroutine( _projectFileName, _projectFolderPath, _saveOnlyXml ) );
    }

    private void SaveAsProject()
    {
        string file = StandaloneFileBrowser.SaveFilePanel( "Save project as...", m_DefaultProjectDir, m_DataManager.MetaData.DataName, Globals.XML_FILE_FILTER );

        file = Path.GetFileNameWithoutExtension( file );

        // Only continue, if one folder was selected
        this.SaveProject( file, m_DefaultProjectDir, false );
    }

    private IEnumerator SaveProjectCoroutine( string _projectFileName, string _projectFolderPath, bool _saveOnlyXml )
    {
        m_SaveProgressBar.fillAmount = 0;
        m_SaveProgressBarText.text = "0 %";
        m_SaveScreen.SetActive( true );

        // We are waiting for two frames so that unity has enough time to redraw the ui
        // which apparently it needs or otherwise the positions are off...
        yield return null;
        yield return null;

        m_DataManager.SaveProject( _projectFileName, _projectFolderPath, _saveOnlyXml, new Progress<float>( progress =>
        {
            m_SaveProgressBar.fillAmount = progress;
            m_SaveProgressBarText.text = $"{( progress * 100 ).ToString( "0" )} %";
        } ), () =>
        {
            m_SaveProjectButton.interactable = true;
            m_SaveProjectAsButton.interactable = false;
            m_SaveScreen.SetActive( false );
        } );
    }
}
