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
        nodes.Add(id, node);
        node.Initalize(this, id);
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

    public Guid Connect(Guid p1, Guid p2)
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

    public Edge RemoveEdge(Guid edgeId)
    {
        Edge edge = edges[edgeId];
        edges.Remove(edgeId);
        ports[edge.portInput].edges.Remove(edgeId);
        ports[edge.portOutput].edges.Remove(edgeId);
        ports[edge.portInput].data = null;
        ports[edge.portOutput].data = null;
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
        for (int i = 0; i < p.edges.Count; i++)
        {
            RemoveEdge(p.edges[0]);
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
        for (int i = 0; i < node.inputPorts.Count; i++)
        {
            for (int j = 0; j < ports[node.inputPorts[0]].edges.Count; j++)
            {
                RemoveEdge(ports[node.inputPorts[0]].edges[0]);
            }
            RemovePort(node.inputPorts[0]);
        }
        for (int i = 0; i < node.outputPorts.Count; i++)
        {
            for (int j = 0; j < ports[node.outputPorts[0]].edges.Count; j++)
            {
                RemoveEdge(ports[node.outputPorts[0]].edges[0]);
            }
            RemovePort(node.inputPorts[0]);
        }
        node.inputPorts.Clear();
        node.outputPorts.Clear();
        return node;
    }

    public void Propagate()
    {
        var indegree = new Dictionary<Guid, int>(nodes.Count);
        foreach (var nid in nodes.Keys)
            indegree[nid] = 0;
        foreach (var e in edges.Values)
        {
            Guid dstNode = ports[e.portInput].parent;
            if (indegree.ContainsKey(dstNode))
                indegree[dstNode]++;
        }
        var q = new Queue<Guid>();
        foreach (var kv in rootNodes)
            q.Enqueue(kv.Key);

        int processed = 0;
        while (q.Count != 0)
        {
            Guid curr = q.Dequeue();
            processed++;

            GraphNode n = nodes[curr];
            n.Evaluate(this);
            n.UI?.SetData();
            foreach (Guid outPortId in n.outputPorts)
            {
                Port outPort = ports[outPortId];

                foreach (Guid eid in outPort.edges)
                {
                    Edge e = edges[eid];
                    ports[e.portInput].data = outPort.data;
                    Guid dstNode = ports[e.portInput].parent;
                    if (indegree.ContainsKey(dstNode))
                    {
                        indegree[dstNode]--;
                        if (indegree[dstNode] == 0)
                            q.Enqueue(dstNode);
                    }
                }
            }
        }
    }
}
