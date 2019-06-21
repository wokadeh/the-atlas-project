public interface IDataAssetBuilder {
    DataAsset Build8Bit(byte[][] data);
    DataAsset Build16Bit(short[][] data);
}
