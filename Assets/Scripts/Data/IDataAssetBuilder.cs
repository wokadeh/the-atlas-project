public interface IDataAssetBuilder {
    EarthDataFrame Build8Bit(byte[][] data);
    EarthDataFrame Build16Bit(short[][] data);
}
