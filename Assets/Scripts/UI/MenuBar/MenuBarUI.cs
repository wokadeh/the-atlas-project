using UnityEngine;
using UnityEngine.UI;
using SFB;

public class MenuBarUI : MonoBehaviour {
    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private TransferFunctionUI m_TransferFunctionUI;
    [SerializeField] private Button m_ImportDataButton;

    private void Start() {
        m_ImportDataButton.onClick.AddListener(ImportData);
    }

    private void ImportData() {
        string[] folders = StandaloneFileBrowser.OpenFolderPanel(null, null, false);
        if (folders.Length == 1) {
            string folder = folders[0];
            DataAsset data = m_DataManager.Load(folder);
            m_VolumeRenderer.SetData(data);
            m_TransferFunctionUI.RedrawHistogram();
        }
    }
}
