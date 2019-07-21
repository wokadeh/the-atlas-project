using System.Collections.Generic;

public interface IMetaData {
    int BitDepth { get; }
    int Width { get; }
    int Height { get; }
    int Levels { get; }

    IList<IVariable> Variables { get; }
}

public interface IVariable {
    string Name { get; }
}