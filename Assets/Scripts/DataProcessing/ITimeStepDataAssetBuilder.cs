using UnityEngine;

public interface ITimeStepDataAssetBuilder
{
    TimeStepDataAsset BuildTimestepDataAssetFromTexture(Texture3D tex);
    TimeStepDataAsset BuildTimestepDataAssetFromData(byte[][] data);
}
