using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class NodeData
{
    public string Id;
    public bool Seen;
    public GameObject Obj;
    public HashSet<string> IncomingEdgeIds;
    public HashSet<string> OutgoingEdgeIds;
}
public class EdgeData
{
    public string Id;
    public string StartId;
    public string EndId;
    public GameObject Obj;
}

public class Lib : MonoBehaviour
{
    public GameObject NodePrefab;
    public GameObject EdgePrefab;
    public Dictionary<string, NodeData> Nodes = new Dictionary<string, NodeData>();
    public Dictionary<string, EdgeData> Edges = new Dictionary<string, EdgeData>();
    public NodeData FocusedNode;
    public int LoadDepth = 2;

    void HydrateNode(NodeData node)
    {
        if (node.Obj == null)
            node.Obj = Instantiate(NodePrefab,  transform, false);
    }

    void HydrateEdge(EdgeData edge)
    {
        if (edge.Obj == null)
            edge.Obj = Instantiate(EdgePrefab,  transform, false);
    }

    NodeData NodeFromId(string id)
    {
        return new NodeData
        {
            Id = id,
            Seen = false,
            IncomingEdgeIds = new HashSet<string>(),
            OutgoingEdgeIds = new HashSet<string>()
        };
    }

    void ExploreNode(NodeData node)
    {
        string[] childIds = Directory.GetDirectories(node.Id);
        foreach (string S in childIds)
        {
            EdgeData Edge;
            string EdgeId = node.Id + "!" + S;
            if (Edges.ContainsKey(EdgeId))
            {
                Edge = Edges[EdgeId];
            }
            else
            {
                Edge = new EdgeData
                {
                    Id = EdgeId,
                    StartId = node.Id,
                    EndId = S
                };
                Edges[EdgeId] = Edge;
                if (!Nodes.ContainsKey(Edge.EndId))
                {
                    Nodes[Edge.EndId] = NodeFromId(Edge.EndId);
                }
            }
            node.OutgoingEdgeIds.Add(EdgeId);
        }
        //string parentId = Regex.Replace(node.Id, @"[\/].+$", "");
    }
    void LoadToDepth()
    {
        foreach (NodeData node in Nodes.Values)
        {
            node.Seen = false;
        }
        HashSet<NodeData> Layer = new HashSet<NodeData>();
        Layer.Add(FocusedNode);
        for (int i = 0; i < LoadDepth; i++)
        {
            HashSet<NodeData> NewLayer = new HashSet<NodeData>();
            foreach (var Node in Layer)
            {
                ExploreNode(Node);
                HydrateNode(Node);
                foreach (var EdgeId in Node.OutgoingEdgeIds)
                {
                    EdgeData Edge = Edges[EdgeId];
                    HydrateEdge(Edge);
                    if (!Nodes[Edge.EndId].Seen)
                    {
                        NewLayer.Add(Nodes[Edge.EndId]);
                        Nodes[Edge.EndId].Seen = true;
                    }
                    if (!Nodes[Edge.StartId].Seen)
                    {
                        NewLayer.Add(Nodes[Edge.StartId]);
                        Nodes[Edge.StartId].Seen = true;
                    }
                }
            }
            Layer = NewLayer;
        }
    }

    void AttachEdges()
    {
        foreach (EdgeData E in Edges.Values)
        {
            if (E.Obj != null && Nodes.ContainsKey(E.StartId) && Nodes[E.StartId].Obj != null
                && Nodes.ContainsKey(E.EndId) && Nodes[E.EndId].Obj != null)
            {
                Vector3 StartPosition = Nodes[E.StartId].Obj.transform.position;
                Vector3 EndPosition = Nodes[E.EndId].Obj.transform.position;
                Vector3 Center = (EndPosition + StartPosition) / 2;
                E.Obj.transform.position = Center;
                E.Obj.transform.LookAt(Nodes[E.StartId].Obj.transform);
                float scale = (EndPosition - StartPosition).magnitude;
                E.Obj.transform.localScale = new Vector3(scale, scale, scale); // shold be global?
            }
        }
    }

    void TreeLayoutDown()
    {
        float SpacingX = 0.5f;
        float SpacingY = 0.5f;
        HashSet<NodeData> Layer = new HashSet<NodeData>();
        Layer.Add(FocusedNode);
        for (int i=0; Layer.Count > 0;i++)
        {
        HashSet<NodeData> NextLayer = new HashSet<NodeData>();
            int NodeIndex = 0;
            foreach (NodeData Node in Layer)
            {
                Node.Obj.transform.position = new Vector3((Layer.Count / 2 + NodeIndex) * SpacingX, i * SpacingY, 0);
                foreach (string edgeId in Node.OutgoingEdgeIds)
                {
                    NodeData End = Nodes[Edges[edgeId].EndId];
                    if (End.Obj != null)
                    {
                        NextLayer.Add(End);
                    }
                }
                NodeIndex++;
            }
        }
    }
    void TreeLayout()
    {
        float SpacingX = 0.5f;
        float SpacingY = 0.5f;
        List<NodeData> roots = new List<NodeData>();
        roots.Add(FocusedNode);
        while (roots[roots.Count-1].IncomingEdgeIds.Count>0){
            NodeData prv=null;
            foreach (string s in roots[roots.Count - 1].IncomingEdgeIds) {
                prv = Nodes[s];
            }
            roots.Add(prv);
        }
        
    }

    NodeData findRoot()
    {
        HashSet<NodeData> roots = new HashSet<NodeData>();
        foreach (NodeData Node in Nodes.Values)
        {
            if (Node.IncomingEdgeIds.Count == 0)
            {
                roots.Add(Node);
            }
        }
        if (roots.Count == 1)
        {
            foreach (NodeData Node in roots)
            {
                return Node;
            }
        }
        return null;
    }

    void Layout()
    {
        TreeLayoutDown();
    }

    // Start is called before the first frame update
    void Start()
    {
        string InitId = @"C:/Users/Tao/ExampleFolder";
        FocusedNode = NodeFromId(InitId);
        Nodes[InitId] = FocusedNode;
    }

    void printState()
    {
        string result = "";
        result += "Nodes";
        foreach (NodeData Node in Nodes.Values)
        {
            result += Node.ToString();
        }
        Debug.Log(result);
    }

    // Update is called once per frame
    void Update()
    {
        LoadToDepth();
        Layout();
        AttachEdges();
        if (Random.Range(0, 1) > 0.95)
            Destroy(this);
    }
}
