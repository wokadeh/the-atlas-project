using UnityEngine.UI;
using UnityEngine;

public class CancelProjectUI : MonoBehaviour
{
    [SerializeField] private Button m_CancelButton;

    // Start is called before the first frame update
    void Start()
    {
        m_CancelButton.onClick.AddListener( CancelProjectScreen );
    }

    // Update is called once per frame
    private void CancelProjectScreen()
    {
        Singleton.GetProjectScreen().SetActive( false );
        Singleton.GetMainScreen().SetActive( true );
    }
}
