﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePanel : MonoBehaviour
{
    public void Toggle (GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }
}
