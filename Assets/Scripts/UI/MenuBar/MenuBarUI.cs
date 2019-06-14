using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SFB;

public class MenuBarUI : MonoBehaviour {
    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private TransferFunctionUI m_TransferFunctionUI;
    [SerializeField] private Button m_ImportDataButton;
    [SerializeField] private GameObject m_LoadingScreen;

    private void Start() {
        m_ImportDataButton.onClick.AddListener(ImportData);
    }

    private void ImportData() {
        string[] folders = StandaloneFileBrowser.OpenFolderPanel(null, null, false);
        if (folders.Length == 1) {
            StartCoroutine(ImportDataCoroutine(folders[0]));
        }
    }

    private IEnumerator ImportDataCoroutine(string folder) {
        m_LoadingScreen.SetActive(true);

        // We are waiting for two frames so that unity has time to redraw the ui
        yield return null;
        yield return null;

        DataAsset data = m_DataManager.Load(folder);
        m_VolumeRenderer.SetData(data);
        m_TransferFunctionUI.Redraw();

        m_LoadingScreen.SetActive(false);
    }
}
