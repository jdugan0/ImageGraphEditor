using System;
using System.Collections.Generic;
using Godot;

public partial class Add : GraphNode
{
    public override void Initalize(Dag dag, Guid id)
    {
        base.Initalize(dag, id);
        Port i1 = new Port(true, id, GraphType.Number);
        Port i2 = new Port(true, id, GraphType.Number);
        Port o1 = new Port(false, id, GraphType.Number);
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

public partial class Constant : GraphNode
{
    public override void Initalize(Dag dag, Guid id)
    {
        base.Initalize(dag, id);
        Port o1 = new Port(false, id, GraphType.Number);
        dag.AddPort(id, o1);
    }

    public override void Evaluate(Dag dag)
    {
        base.Evaluate(dag);
    }

    public override void SetData(Dag dag, Dictionary<string, object> dict)
    {
        Port o1 = dag.ports[outputPorts[0]];
        o1.data = dict["value"];
        base.SetData(dag, dict);
    }
}
