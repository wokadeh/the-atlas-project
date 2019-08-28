using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Little helper panel that sets a panel active and inactive
public class TogglePanel : MonoBehaviour
{
    public void Toggle( GameObject panel )
    {
        panel.SetActive( !panel.activeSelf );
    }
}
