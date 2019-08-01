using UnityEngine;
using UnityEngine.UI;

public class ProjectScreenUI : MonoBehaviour
{
    [SerializeField] private Button m_ProjectButton;
    [SerializeField] private GameObject m_ProjectScreen;

    // Start is called before the first frame update
    void Start()
    {
        m_ProjectButton.onClick.AddListener(ShowProjectScreen);
    }

    private void ShowProjectScreen()
    {
        m_ProjectScreen.SetActive(true);
    }
}
