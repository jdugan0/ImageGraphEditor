using System;
using Godot;

public partial class Add : GraphNode
{
    public override void Initalize(Dag dag, Guid id)
    {
        base.Initalize(dag, id);
        Port i1 = new Port(true, id, GraphType.Int);
        Port i2 = new Port(true, id, GraphType.Int);
        Port o1 = new Port(false, id, GraphType.Int);
        dag.AddPort(id, i1);
        dag.AddPort(id, i2);
        dag.AddPort(id, o1);
    }

    public override void Evaluate(Dag dag)
    {
        Port i1 = dag.ports[inputPorts[0]];
        Port i2 = dag.ports[inputPorts[1]];
        Port o1 = dag.ports[outputPorts[0]];
        o1.data = (int)i1.data + (int)i2.data;
        base.Evaluate(dag);
    }
}
