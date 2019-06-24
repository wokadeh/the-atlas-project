using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataTypeUI : MonoBehaviour {
    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private Toggle m_DataTypeTogglePrefab;

    private bool m_Initialized;

    private void Start() {
        
        Initialize();
    }

    private void OnNewImport() {
        // Clean up any old toggles
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Create toggles for all variables
        foreach (IVariable variable in m_DataManager.MetaData.Variables) {
            string name = variable.Name;
            Toggle toggle = Instantiate(m_DataTypeTogglePrefab, transform);
            toggle.isOn = name == m_DataManager.CurrentVariable;

            TMP_Text label = toggle.transform.Find("Label").GetComponent<TMP_Text>();
            label.text = name;

            toggle.onValueChanged.AddListener(isOn => {
                if (isOn) {
                    m_DataManager.SetCurrentVariable(name);
                    Toggle[] toggles = transform.GetComponentsInChildren<Toggle>();
                    if (toggles.Length > 1) {
                        foreach (Toggle t in toggles) {
                            if (t != toggle) {
                                t.isOn = false;
                            }
                        }
                    }
                }
            });
        }
    }

    private void Initialize() {
        if (m_Initialized) {
            return;
        }

        m_DataManager.OnNewImport += OnNewImport;

        if (m_DataManager.CurrentAsset != null) {
            OnNewImport();
        }

        m_Initialized = true;
    }
}
