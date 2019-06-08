using UnityEngine;
using UnityEngine.UI;
using SFB;

public class MenuBarUI : MonoBehaviour {
    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private Button m_ImportDataButton;

    private void Start() {
        m_ImportDataButton.onClick.AddListener(ImportData);
    }

    private void ImportData() {
        string[] folders = StandaloneFileBrowser.OpenFolderPanel(null, null, false);
        if (folders.Length == 1) {
            string folder = folders[0];
            m_DataManager.Load(folder);
        }
    }
}
