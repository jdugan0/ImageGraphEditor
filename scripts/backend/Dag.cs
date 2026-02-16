using System;
using System.Collections.Generic;

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
    public List<Guid> edges;
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

    public Guid AddNode(GraphNode node)
    {
        Guid id = Guid.NewGuid();
        node.Initalize(this, id);
        nodes.Add(id, node);
        if (node.inputPorts.Count == 0)
        {
            rootNodes.Add(id, node);
        }

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

    public void TryConnect(Port input, Port output)
    {
        if (!input.isInput || output.isInput)
        {
            throw new Exception();
        }
        if (input.edges.Count > 0)
        {
            throw new Exception();
        }
        if (input.type != output.type)
        {
            throw new Exception();
        }
        if (AreNodesConnected(input.parent, output.parent))
        {
            throw new Exception();
        }
    }

    public Guid Connect(Guid portInput, Guid portOutput)
    {
        Port input = ports[portInput];
        Port output = ports[portOutput];
        TryConnect(input, output);
        Guid id = Guid.NewGuid();
        Edge edge = new Edge(id, portInput, portOutput);
        edges.Add(id, edge);
        input.edges.Add(id);
        output.edges.Add(id);
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
                        return false;
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

    public Edge RemoveEdge(Guid edgeId)
    {
        Edge edge = edges[edgeId];
        edges.Remove(edgeId);
        ports[edge.portInput].edges.Remove(edgeId);
        ports[edge.portOutput].edges.Remove(edgeId);
        return edge;
    }

    public Port RemovePort(Guid portId)
    {
        Port p = ports[portId];
        if (p.isInput)
        {
            nodes[p.parent].inputPorts.Remove(portId);
        }
        else
        {
            nodes[p.parent].outputPorts.Remove(portId);
        }
        foreach (Guid eid in p.edges)
        {
            RemoveEdge(eid);
        }
        return p;
    }

    public GraphNode RemoveNode(Guid nodeId)
    {
        GraphNode node = nodes[nodeId];
        nodes.Remove(nodeId);
        if (node.inputPorts.Count == 0)
        {
            rootNodes.Remove(nodeId);
        }
        foreach (Guid port in node.inputPorts)
        {
            foreach (Guid eid in ports[port].edges)
            {
                RemoveEdge(eid);
            }
        }
        foreach (Guid port in node.outputPorts)
        {
            foreach (Guid eid in ports[port].edges)
            {
                RemoveEdge(eid);
            }
        }
        node.inputPorts.Clear();
        node.outputPorts.Clear();
        return node;
    }

    public void Propegate()
    {
        Queue<Guid> nodeQueue = new Queue<Guid>();
        HashSet<Guid> seen = new HashSet<Guid>();
        foreach (var x in rootNodes.Keys)
        {
            nodeQueue.Enqueue(x);
            seen.Add(x);
        }
        while (nodeQueue.Count != 0)
        {
            Guid curr = nodeQueue.Dequeue();
            GraphNode n = nodes[curr];
            n.Evaluate(this);
            foreach (Guid portid in n.outputPorts)
            {
                Port p = ports[portid];
                foreach (Guid eid in p.edges)
                {
                    Edge e = edges[eid];
                    Guid parent = ports[e.portInput].parent;
                    if (!seen.Contains(parent))
                    {
                        ports[e.portInput].data = ports[e.portOutput].data;
                        seen.Add(parent);
                        nodeQueue.Enqueue(parent);
                    }
                }
            }
        }
    }
}
