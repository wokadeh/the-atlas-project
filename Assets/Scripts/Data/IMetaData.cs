using System.Collections.Generic;

public interface IMetaData {
    string DataName { get; }
    int BitDepth { get; }
    int Width { get; }
    int Height { get; }
    int Levels { get; }
    float StartDateTimeNumber { get; }
    float EndDateTimeNumber { get; }
    int TimeInterval { get; }

    IList<IVariable> Variables { get; }
    IList<IList<ITimestamp>> Timestamps { get; set; }
}

public interface IVariable {
    string Name { get; }
}

public interface ITimestamp
{
    float Value { get; }
}