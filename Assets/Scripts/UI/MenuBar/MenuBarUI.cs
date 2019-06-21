using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System;

public class MenuBarUI : MonoBehaviour {
    private static readonly ExtensionFilter[] FILE_FILTER = new ExtensionFilter[] {
        new ExtensionFilter("Xml File", "xml")
    };

    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private TransferFunctionUI m_TransferFunctionUI;
    [SerializeField] private Button m_ImportDataButton;
    [SerializeField] private GameObject m_LoadingScreen;

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
        m_LoadingScreen.SetActive(true);

        // We are waiting for two frames so that unity has time to redraw the ui
        yield return null;
        yield return null;

        m_DataManager.Load(file, new Progress<float>(progress => {
            // TODO: Show progress bar
            Debug.Log($"Progress report: {progress}");
        }), (success, asset) => {
            if (success) {
                m_VolumeRenderer.SetData(asset);
                m_TransferFunctionUI.Redraw();
            }
            m_LoadingScreen.SetActive(false);
        });
    }
}
