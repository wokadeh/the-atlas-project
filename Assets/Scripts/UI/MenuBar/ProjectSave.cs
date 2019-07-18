using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using TMPro;

public class ProjectSave : MonoBehaviour
{
    private static readonly ExtensionFilter[] FILE_FILTER = new ExtensionFilter[] {
        new ExtensionFilter("Xml File", "xml")
    };

    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private GameObject m_TransferFunctionUIPanel;
    [SerializeField] private Button m_SaveProjectButton;

    // Start is called before the first frame update
    void Start()
    {
        m_SaveProjectButton.onClick.AddListener(SaveProject);
    }

    private void SaveProject()
    {
        string[] files = StandaloneFileBrowser.OpenFilePanel("Open data xml", null, FILE_FILTER, false);

        // This is a little hackey but works for now
        m_TransferFunctionUIPanel.SetActive(false);

        if (files.Length == 1)
        {
            StartCoroutine(SaveProjectCoroutine(files[0]));
        }
    }

    private IEnumerator SaveProjectCoroutine(string file)
    {
        m_DataManager.Load(file, new Progress<float>(progress => {
            m_ImportPorgressBar.fillAmount = progress;
            m_ImportProgressBarText.text = $"{(progress * 100).ToString("0")} %";
        }), () => {
            m_ImportScreen.SetActive(false);
        });
    }
}
