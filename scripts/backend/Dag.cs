using System;
using System.Collections.Generic;
using Godot;

public enum GraphType
{
    Number,
    Image,
    Null,
}

public abstract class GraphNode
{
    public readonly List<Guid> outputPorts = new List<Guid>();
    public readonly List<Guid> inputPorts = new List<Guid>();

    public readonly Dictionary<string, object> data = new Dictionary<string, object>();

    public Guid id;

    public NodeUI UI;

    public virtual void Evaluate(Dag dag) { }

    public virtual void Initalize(Dag dag, Guid id)
    {
        this.id = id;
    }

    public virtual void SetData(Dag dag, Dictionary<string, object> dict) { }
}

public class Port
{
    public bool isInput;
    public List<Guid> edges = new List<Guid>();
    public Guid parent;
    public GraphType type = GraphType.Null;
    public object data = null;

    public Port(bool isInput, Guid parent, GraphType graphType)
    {
        this.isInput = isInput;
        this.parent = parent;
        this.type = graphType;
    }
}

public class Edge
{
    public Guid id;
    public Guid portInput;
    public Guid portOutput;

    public EdgeUI UI;

    public Edge(Guid id, Guid input, Guid output)
    {
        this.id = id;
        this.portInput = input;
        this.portOutput = output;
    }
}

public class Dag
{
    public readonly Dictionary<Guid, Port> ports = new Dictionary<Guid, Port>();
    public readonly Dictionary<Guid, GraphNode> nodes = new Dictionary<Guid, GraphNode>();
    public readonly Dictionary<Guid, Edge> edges = new Dictionary<Guid, Edge>();
    public readonly Dictionary<Guid, GraphNode> rootNodes = new Dictionary<Guid, GraphNode>();
    private Queue<Guid> q = new Queue<Guid>();
    private readonly HashSet<Guid> dirty = new HashSet<Guid>();

    public void MarkDirty(Guid nodeId)
    {
        if (nodes.ContainsKey(nodeId))
            dirty.Add(nodeId);
    }

    public Guid AddNode(GraphNode node)
    {
        Guid id = Guid.NewGuid();
        nodes.Add(id, node);
        node.Initalize(this, id);
        if (node.inputPorts.Count == 0)
        {
            rootNodes.Add(id, node);
        }
        MarkDirty(id);
        return id;
    }

    public Guid AddPort(Guid nodeId, Port port)
    {
        GraphNode node = nodes[nodeId];
        Guid id = Guid.NewGuid();
        ports.Add(id, port);
        port.parent = nodeId;
        if (port.isInput)
        {
            node.inputPorts.Add(id);
        }
        else
        {
            node.outputPorts.Add(id);
        }
        return id;
    }

    public void TryConnect(Guid p1, Guid p2)
    {
        Port input;
        Port output;
        if (ports[p1].isInput)
        {
            input = ports[p1];
            output = ports[p2];
        }
        else
        {
            input = ports[p2];
            output = ports[p1];
        }
        if (p1 == p2)
        {
            throw new Exception("Cannot connect node to itself");
        }
        if (!input.isInput || output.isInput)
        {
            throw new Exception("Not connecting output -> input");
        }
        if (input.edges.Count > 0)
        {
            throw new Exception("Input already connected.");
        }
        if (input.type != output.type)
        {
            throw new Exception("Types don't match.");
        }
        if (AreNodesConnected(input.parent, output.parent))
        {
            throw new Exception("Loop found.");
        }
    }

    public Guid Connect(Guid p1, Guid p2, EdgeUI UI)
    {
        Port input;
        Port output;
        Guid inputId;
        Guid outputId;
        if (ports[p1].isInput)
        {
            input = ports[p1];
            inputId = p1;
            output = ports[p2];
            outputId = p2;
        }
        else
        {
            input = ports[p2];
            inputId = p2;
            output = ports[p1];
            outputId = p1;
        }
        TryConnect(p1, p2);
        Guid id = Guid.NewGuid();
        Edge edge = new Edge(id, inputId, outputId);
        edge.UI = UI;
        edges.Add(id, edge);
        input.edges.Add(id);
        output.edges.Add(id);
        MarkDirty(output.parent);
        return id;
    }

    public bool AreNodesConnected(Guid n1, Guid n2)
    {
        Queue<Guid> nodeQueue = new Queue<Guid>();
        nodeQueue.Enqueue(n1);
        HashSet<Guid> seen = [n1];
        while (nodeQueue.Count != 0)
        {
            Guid curr = nodeQueue.Dequeue();
            GraphNode n = nodes[curr];
            foreach (Guid portid in n.outputPorts)
            {
                Port p = ports[portid];
                foreach (Guid eid in p.edges)
                {
                    Edge e = edges[eid];
                    Guid parent = ports[e.portInput].parent;
                    if (parent.Equals(n2))
                    {
                        return true;
                    }
                    if (!seen.Contains(parent))
                    {
                        seen.Add(parent);
                        nodeQueue.Enqueue(parent);
                    }
                }
            }
        }
        return false;
    }

    public Edge RemoveEdge(Guid edgeId, bool enqueue)
    {
        Edge edge = edges[edgeId];
        edges.Remove(edgeId);
        ports[edge.portInput].edges.Remove(edgeId);
        ports[edge.portOutput].edges.Remove(edgeId);
        ports[edge.portInput].data = null;
        ports[edge.portOutput].data = null;
        edge.UI?.QueueFree();
        MarkDirty(ports[edge.portInput].parent);
        MarkDirty(ports[edge.portOutput].parent);
        return edge;
    }

    public Port RemovePort(Guid portId)
    {
        Port p = ports[portId];
        if (nodes.ContainsKey(p.parent))
        {
            if (p.isInput)
            {
                nodes[p.parent].inputPorts.Remove(portId);
            }
            else
            {
                nodes[p.parent].outputPorts.Remove(portId);
            }
        }
        for (int i = 0; i < p.edges.Count; i++)
        {
            RemoveEdge(p.edges[0], false);
        }
        ports.Remove(portId);
        return p;
    }

    public GraphNode RemoveNode(Guid nodeId)
    {
        if (!nodes.TryGetValue(nodeId, out GraphNode node))
            return null;
        rootNodes.Remove(nodeId);
        var inPorts = node.inputPorts.ToArray();
        var outPorts = node.outputPorts.ToArray();
        foreach (var pid in inPorts)
            RemovePort(pid);
        foreach (var pid in outPorts)
            RemovePort(pid);
        nodes.Remove(nodeId);
        node.inputPorts.Clear();
        node.outputPorts.Clear();

        return node;
    }

    public void Propagate()
    {
        if (dirty.Count == 0)
            return;

        var affected = new HashSet<Guid>();
        var frontier = new Queue<Guid>();

        foreach (var n in dirty)
        {
            if (nodes.ContainsKey(n) && affected.Add(n))
                frontier.Enqueue(n);
        }

        while (frontier.Count != 0)
        {
            var curr = frontier.Dequeue();
            var node = nodes[curr];

            foreach (var outPid in node.outputPorts)
            {
                var outPort = ports[outPid];
                foreach (var eid in outPort.edges)
                {
                    var e = edges[eid];
                    var child = ports[e.portInput].parent;
                    if (nodes.ContainsKey(child) && affected.Add(child))
                        frontier.Enqueue(child);
                }
            }
        }

        var remaining = new Dictionary<Guid, int>(affected.Count);
        foreach (var nid in affected)
            remaining[nid] = 0;

        foreach (var nid in affected)
        {
            var node = nodes[nid];
            foreach (var inPid in node.inputPorts)
            {
                var inPort = ports[inPid];
                foreach (var eid in inPort.edges)
                {
                    var e = edges[eid];
                    var parent = ports[e.portOutput].parent;
                    if (affected.Contains(parent))
                        remaining[nid]++;
                }
            }
        }

        var ready = new Queue<Guid>();
        foreach (var kv in remaining)
            if (kv.Value == 0)
                ready.Enqueue(kv.Key);

        int processed = 0;

        while (ready.Count != 0)
        {
            var curr = ready.Dequeue();
            if (!nodes.ContainsKey(curr))
                continue;
            processed++;

            var n = nodes[curr];

            try
            {
                n.Evaluate(this);
                n.UI?.SetData();
                n.UI?.SucceedEval();
            }
            catch (Exception ex)
            {
                n.UI?.FailedEval(ex.Message);
            }
            foreach (var outPid in n.outputPorts)
            {
                var outPort = ports[outPid];

                foreach (var eid in outPort.edges)
                {
                    var e = edges[eid];

                    ports[e.portInput].data = outPort.data;

                    var child = ports[e.portInput].parent;
                    if (!affected.Contains(child))
                        continue;

                    remaining[child]--;
                    if (remaining[child] == 0)
                        ready.Enqueue(child);
                }
            }
        }

        dirty.Clear();
    }
}
