using System.Collections.Generic;

public interface IMetaData {
    string DataName { get; }
    int BitDepth { get; }
    int Width { get; }
    int Height { get; }
    int Levels { get; }

    IList<IVariable> Variables { get; }
}

public interface IVariable {
    string Name { get; }
}