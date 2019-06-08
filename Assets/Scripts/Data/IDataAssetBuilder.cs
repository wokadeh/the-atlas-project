using UnityEngine;

public interface IDataAssetBuilder {
    DataAsset Build(Texture2D[] textures);
}
