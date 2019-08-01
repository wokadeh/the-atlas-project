using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using TMPro;

public class DataImportUI : MonoBehaviour {
    private static readonly ExtensionFilter[] FILE_FILTER = new ExtensionFilter[] {
        new ExtensionFilter("Xml File", "xml")
    };

    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private GameObject m_TransferFunctionUIPanel;
    [SerializeField] private GameObject m_TimelineUIPanel;
    [SerializeField] private Button m_ImportDataButton;
    [SerializeField] private GameObject m_ImportScreen;
    [SerializeField] private Image m_ImportProgressBar;
    [SerializeField] private TMP_Text m_ImportProgressBarText;
    [SerializeField] private GameObject m_ProjectScreen;

    private void Start() {
        m_ImportDataButton.onClick.AddListener(ImportData);
    }

    private void ImportData() {
        string[] files = StandaloneFileBrowser.OpenFilePanel("Open data xml", null, FILE_FILTER, false);

        // This is a little hackey but works for now
        m_TransferFunctionUIPanel.SetActive(false);
        m_TimelineUIPanel.SetActive(false);

        if (files.Length == 1) {
            StartCoroutine(ImportDataCoroutine(files[0]));
        }
    }

    private IEnumerator ImportDataCoroutine(string file) {
        m_ImportProgressBar.fillAmount = 0;
        m_ImportProgressBarText.text = "0 %";
        m_ImportScreen.SetActive(true);

        // We are waiting for two frames so that unity has enough time to redraw the ui
        // which apparently it needs or otherwise the positions are off...
        yield return null;
        yield return null;

        m_DataManager.ImportData(file, new Progress<float>(progress => {
            m_ImportProgressBar.fillAmount = progress;
            m_ImportProgressBarText.text = $"{(progress * 100).ToString("0")} %";
        }), () => {
            m_ImportScreen.SetActive(false);
        });
    }
}
