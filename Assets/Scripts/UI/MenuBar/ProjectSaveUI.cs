using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using TMPro;
using System.IO;

public class ProjectSaveUI : MonoBehaviour
{
    private static readonly ExtensionFilter[] FILE_FILTER = new ExtensionFilter[] {
        new ExtensionFilter("Xml File", "xml")
    };

    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private GameObject m_TransferFunctionUIPanel;
    [SerializeField] private GameObject m_SaveScreen;
    [SerializeField] private Image m_SaveProgressBar;
    [SerializeField] private TMP_Text m_SaveProgressBarText;
    [SerializeField] private Button m_SaveProjectButton;

    // Start is called before the first frame update
    void Start()
    {
        m_SaveProjectButton.onClick.AddListener(SaveProject);
    }

    private void SaveProject()
    {
        string[] folders = StandaloneFileBrowser.OpenFolderPanel("Save project at...", Directory.GetCurrentDirectory(), false);

        string projectFolderPath = folders[0];

        Debug.Log("[ProjectSaveUI] - selected folder " + projectFolderPath);

        // This is a little hackey but works for now
        m_TransferFunctionUIPanel.SetActive(false);

        StartCoroutine(SaveProjectCoroutine(projectFolderPath));
    }

    private IEnumerator SaveProjectCoroutine(string _projectFolderPath)
    {
        // We are waiting for two frames so that unity has enough time to redraw the ui
        // which apparently it needs or otherwise the positions are off...
        yield return null;
        yield return null;

        m_DataManager.SaveProject(_projectFolderPath, new Progress<float>(progress => {
            m_SaveProgressBar.fillAmount = progress;
            m_SaveProgressBarText.text = $"{(progress * 100).ToString("0")} %";
        }), () => {
            m_SaveScreen.SetActive(false);
        });
    }
}
