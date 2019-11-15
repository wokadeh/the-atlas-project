using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using TMPro;
using System.IO;

public class DataImportUI : MonoBehaviour
{
    private static readonly ExtensionFilter[] FILE_FILTER = new ExtensionFilter[] {
        new ExtensionFilter("Xml File", "xml")
    };

    [SerializeField] private GameObject m_TransferFunctionUIPanel;
    [SerializeField] private GameObject m_TimelineUIPanel;
    [SerializeField] private Button m_ImportDataButton;
    [SerializeField] private GameObject m_ImportScreen;
    [SerializeField] private Image m_ImportProgressBar;
    [SerializeField] private TMP_Text m_ImportProgressBarText;
    [SerializeField] private Button m_SaveProjectButton;
    [SerializeField] private Button m_CancelButton;
    [SerializeField] private Button m_SaveProjectAsButton;

    private void Start()
    {
        m_ImportDataButton.onClick.AddListener( ImportData );
    }

    private void ImportData()
    {
        string[] files = StandaloneFileBrowser.OpenFilePanel("Open data xml", Directory.GetCurrentDirectory() + "\\" + Globals.IMPORT_DATA_PATH, FILE_FILTER, false);

        // This is a little hackey but works for now
        m_TransferFunctionUIPanel.SetActive( false );
        m_TimelineUIPanel.SetActive( false );

        if( files.Length == 1 )
        {
            StartCoroutine( ImportDataCoroutine( files[ 0 ] ) );
        }
    }

    private IEnumerator ImportDataCoroutine( string file )
    {
        yield return Utils.SetupProjectCoroutine( m_ImportScreen, Singleton.GetProjectScreen(), m_ImportProgressBar, m_ImportProgressBarText );

        Singleton.GetDataManager().ImportData( file, Utils.CreateProgressBarProgress( m_ImportProgressBar, m_ImportProgressBarText ), () =>
        {
            Utils.SetupScreenWhileProgress( m_ImportScreen, Singleton.GetMainScreenSystem(), Singleton.GetBottomScreen(), m_SaveProjectButton, m_SaveProjectAsButton, m_CancelButton );
        } );
    }
}
