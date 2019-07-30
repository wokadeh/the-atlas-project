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

    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private GameObject m_TransferFunctionUIPanel;
    [SerializeField] private GameObject m_LoadScreen;
    [SerializeField] private Image m_LoadProgressBar;
    [SerializeField] private TMP_Text m_LoadProgressBarText;
    [SerializeField] private Button m_LoadProjectButton;

    // Start is called before the first frame update
    void Start()
    {
        m_LoadProjectButton.onClick.AddListener(LoadProject);
    }

    private void LoadProject()
    {
        string[] folders = StandaloneFileBrowser.OpenFilePanel("Load project file...", Directory.GetCurrentDirectory() + "/" + Globals.SAVE_PROJECTS_PATH, FILE_FILTER, false);

        // Only continue, if one folder was selected
        if(folders.Length > 0)
        {
            string projectFolderPath = folders[0] + "/";

            Log.Info(this, "Load from project file " + projectFolderPath);

            // This is a little hackey but works for now
            m_TransferFunctionUIPanel.SetActive(false);

            this.StartCoroutine(LoadProjectCoroutine(projectFolderPath));
        }
    }

    private IEnumerator LoadProjectCoroutine(string _projectFolderPath)
    {
        // We are waiting for two frames so that unity has enough time to redraw the ui
        // which apparently it needs or otherwise the positions are off...
        yield return null;
        yield return null;

        m_DataManager.LoadProject(_projectFolderPath, new Progress<float>(progress => {
            m_LoadProgressBar.fillAmount = progress;
            m_LoadProgressBarText.text = $"{(progress * 100).ToString("0")} %";
        }), () => {
            m_LoadScreen.SetActive(false);
        });
    }
}
