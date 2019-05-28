using UnityEngine;
using UnityEngine.UI;
using SFB;

public class MenuBarUI : MonoBehaviour {
    [SerializeField] private Button m_ImportDataButton;

    private void Start() {
        m_ImportDataButton.onClick.AddListener(DummyLoadData);
    }

    private void DummyLoadData() {
        string[] folders = StandaloneFileBrowser.OpenFolderPanel(null, null, false);
        if (folders.Length == 1) {
            string folder = folders[0];
            Debug.Log($"Import data from folder: {folder}");
            // TODO: Will call some sort of data loader
            // IDataLoader dataLoader = new DataLoader();
            // dataLoader.Load(folder);
        }
    }
}
