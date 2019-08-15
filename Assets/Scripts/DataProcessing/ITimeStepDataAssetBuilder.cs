using UnityEngine;

public interface ITimeStepDataAssetBuilder
{
    TimeStepDataAsset BuildTimestepDataAssetFromTexture(Texture3D tex);
    TimeStepDataAsset BuildTimestepDataAssetFromData(byte[][] data);
    TimeStepDataAsset Build16Bit(short[][] data);
}
