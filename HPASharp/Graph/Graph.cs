﻿using System;
using System.Collections.Generic;
using System.Xml;
using HPASharp.Infrastructure;

namespace HPASharp
{
	public interface INode<Tid, TInfo, TEdge>
	{
		Id<Tid> NodeId { get; set; }
		TInfo Info { get; set; }
		List<TEdge> Edges { get; set; }

		void RemoveEdge(Id<Tid> targetNodeId);
	}

	public interface IEdge<TNode, TEdgeInfo>
	{
		Id<TNode> TargetNodeId { get; set; }
		TEdgeInfo Info { get; set; }
	}

	public class AbstractNode : INode<AbstractNode, AbstractNodeInfo, AbstractEdge>
	{
		public Id<AbstractNode> NodeId { get; set; }
		public AbstractNodeInfo Info { get; set; }
		public List<AbstractEdge> Edges { get; set; }

		public AbstractNode(Id<AbstractNode> nodeId, AbstractNodeInfo info)
		{
			NodeId = nodeId;
			Info = info;
			Edges = new List<AbstractEdge>();
		}

		public void RemoveEdge(Id<AbstractNode> targetNodeId)
		{
			Edges.RemoveAll(e => e.TargetNodeId == targetNodeId);
		}

		public static INode<AbstractNode, AbstractNodeInfo, AbstractEdge> CreateNode(Id<AbstractNode> nodeId,
			AbstractNodeInfo info)
		{
			return new AbstractNode(nodeId, info);
		}
	}

	public class AbstractEdge : IEdge<AbstractNode, AbstractEdgeInfo>
	{
		public Id<AbstractNode> TargetNodeId { get; set; }
		public AbstractEdgeInfo Info { get; set; }

		public AbstractEdge(Id<AbstractNode> targetNodeId, AbstractEdgeInfo info)
		{
			TargetNodeId = targetNodeId;
			Info = info;
		}
	}

	public class ConcreteNode : INode<ConcreteNode, ConcreteNodeInfo, ConcreteEdge>
	{
		public Id<ConcreteNode> NodeId { get; set; }
		public ConcreteNodeInfo Info { get; set; }
		public List<ConcreteEdge> Edges { get; set; }

		public ConcreteNode(Id<ConcreteNode> nodeId, ConcreteNodeInfo info)
		{
			NodeId = nodeId;
			Info = info;
			Edges = new List<ConcreteEdge>();
		}

		public void RemoveEdge(Id<ConcreteNode> targetNodeId)
		{
			Edges.RemoveAll(e => e.TargetNodeId == targetNodeId);
		}
	}

	public class ConcreteEdge : IEdge<ConcreteNode, ConcreteEdgeInfo>
	{
		public Id<ConcreteNode> TargetNodeId { get; set; }
		public ConcreteEdgeInfo Info { get; set; }

		public ConcreteEdge(Id<ConcreteNode> targetNodeId, ConcreteEdgeInfo info)
		{
			TargetNodeId = targetNodeId;
			Info = info;
		}
	}

	/// <summary>
	/// A graph is a set of nodes connected with edges. Each node and edge can hold
	/// a certain amount of information, which is expressed in the templated parameters
	/// NODEINFO and EDGEINFO
	/// </summary>
	public class Graph<TNode, TNodeInfo, TEdge, TEdgeInfo> 
		where TNode : INode<TNode, TNodeInfo, TEdge>
		where TEdge : IEdge<TNode, TEdgeInfo>
	{
		// We store the nodes in a list because the main operations we use
		// in this list are additions, random accesses and very few removals (only when
		// adding or removing nodes to perform specific searches).
		// This list is implicitly indexed by the nodeId, which makes removing a random
		// Node in the list quite of a mess. We could use a dictionary to ease removals,
		// but lists and arrays are faster for random accesses, and we need performance.
        public List<TNode> Nodes { get; set; }

	    private readonly Func<Id<TNode>, TNodeInfo, TNode> _nodeCreator;
		private readonly Func<Id<TNode>, TEdgeInfo, TEdge> _edgeCreator;

		public Graph(Func<Id<TNode>, TNodeInfo, TNode> nodeCreator, Func<Id<TNode>, TEdgeInfo, TEdge> edgeCreator)
        {
            Nodes = new List<TNode>();
	        _nodeCreator = nodeCreator;
	        _edgeCreator = edgeCreator;
        } 

		/// <summary>
		/// Adds or updates a node with the provided info. A node is updated
		/// only if the nodeId provided previously existed.
		/// </summary>
        public void AddNode(Id<TNode> nodeId, TNodeInfo info)
        {
            var size = nodeId + 1;
            if (Nodes.Count < size)
                Nodes.Add(_nodeCreator(nodeId, info));
            else
                Nodes[nodeId] = _nodeCreator(nodeId, info);
        }

		#region AbstractGraph updating

		public void AddEdge(Id<TNode> sourceNodeId, Id<TNode> targetNodeId, TEdgeInfo info)
        {
            Nodes[sourceNodeId].Edges.Add(_edgeCreator(targetNodeId, info));
        }
        
        public void RemoveEdgesFromNode(Id<TNode> nodeId)
        {
            foreach (var edge in Nodes[nodeId].Edges)
            {
                Nodes[edge.TargetNodeId].RemoveEdge(nodeId);
            }

            Nodes[nodeId].Edges.Clear();
        }

        public void RemoveLastNode()
        {
            Nodes.RemoveAt(Nodes.Count - 1);
        }

        #endregion

        public TNode GetNode(int nodeId)
        {
            return Nodes[nodeId];
        }

        public TNodeInfo GetNodeInfo(int nodeId)
        {
            return GetNode(nodeId).Info;
        }
        
        public List<TEdge> GetEdges(int nodeId)
        {
            return Nodes[nodeId].Edges;
        }
    }

	public class ConcreteGraph : Graph<ConcreteNode, ConcreteNodeInfo, ConcreteEdge, ConcreteEdgeInfo>
	{
		public ConcreteGraph() : base((nodeid, info) => new ConcreteNode(nodeid, info), (nodeid, info) => new ConcreteEdge(nodeid, info))
		{
		}
	}

	public class AbstractGraph : Graph<AbstractNode, AbstractNodeInfo, AbstractEdge, AbstractEdgeInfo>
	{
		public AbstractGraph() : base((nodeid, info) => new AbstractNode(nodeid, info), (nodeid, info) => new AbstractEdge(nodeid, info))
		{
		}
	}
}