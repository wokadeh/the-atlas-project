using UnityEngine.UI;
using UnityEngine;

public class CancelProjectUI : MonoBehaviour
{
    [SerializeField] private GameObject m_ProjectScreen;
    [SerializeField] private Button m_CancelButton;

    // Start is called before the first frame update
    void Start()
    {
        m_CancelButton.onClick.AddListener(CencelProjectScreen);
    }

    // Update is called once per frame
    private void CencelProjectScreen()
    {
        m_ProjectScreen.SetActive(false);
    }
}
