// ****************************** LOCATION ********************************
//
// [UI] Activate_Spherical -> attached
//
// ************************************************************************

using UnityEngine;
using UnityEngine.UI;

public class ModeUI : MonoBehaviour {

    [SerializeField] private Button m_SphericalActivateButton;

    private void Start()
    {
        m_SphericalActivateButton.onClick.AddListener( () => {
            if( Singleton.GetVolumeRenderer().Mode != VolumeRendererMode.Cartesian )
            {
                Singleton.GetVolumeRenderer().SetMode( VolumeRendererMode.Cartesian );
            }
            else
            {
                Singleton.GetVolumeRenderer().SetMode( VolumeRendererMode.Spherical );
            }
        } );
    }
}
