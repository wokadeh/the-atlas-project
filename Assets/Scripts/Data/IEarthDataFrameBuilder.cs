public interface IEarthDataFrameBuilder {
    EarthDataFrame Build8Bit(byte[][] data);
    EarthDataFrame Build16Bit(short[][] data);
}
