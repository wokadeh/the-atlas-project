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
    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private TransferFunctionUI m_TransferFunctionUI;
    [SerializeField] private Button m_ImportDataButton;
    [SerializeField] private GameObject m_ImportScreen;
    [SerializeField] private Image m_ImportPorgressBar;
    [SerializeField] private TMP_Text m_ImportProgressBarText;

    private void Start() {
        m_ImportDataButton.onClick.AddListener(ImportData);
    }

    private void ImportData() {
        string[] files = StandaloneFileBrowser.OpenFilePanel("Open data xml", null, FILE_FILTER, false);
        if (files.Length == 1) {
            StartCoroutine(ImportDataCoroutine(files[0]));
        }
    }

    private IEnumerator ImportDataCoroutine(string file) {
        m_ImportPorgressBar.fillAmount = 0;
        m_ImportProgressBarText.text = "0 %";
        m_ImportScreen.SetActive(true);

        // We are waiting for two frames so that unity has enough time to redraw the ui
        // which apparently it needs or otherwise the positions are off...
        yield return null;
        yield return null;

        m_DataManager.Load(file, new Progress<float>(progress => {
            m_ImportPorgressBar.fillAmount = progress;
            m_ImportProgressBarText.text = $"{(progress * 100).ToString("0")} %";
        }), (success, asset) => {
            if (success) {
                m_VolumeRenderer.SetData(asset);
                m_TransferFunctionUI.Redraw();
            }
            m_ImportScreen.SetActive(false);
        });
    }
}
