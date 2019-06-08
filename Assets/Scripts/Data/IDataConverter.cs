using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataConverter {
    Texture3D Convert(Texture2D[] textures);
}
