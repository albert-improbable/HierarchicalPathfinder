﻿namespace HPASharp.Factories
{
	public class GraphFactory
	{
		public static Graph<ConcreteNodeInfo, ConcreteEdgeInfo> CreateGraph(int width, int height, IPassability passability)
		{
			var graph = new Graph<ConcreteNodeInfo, ConcreteEdgeInfo>();

			CreateNodes(width, height, graph, passability);
			CreateEdges(graph, width, height, TileType.Octile); // We hardcode OCTILE for the time being

			return graph;
		}

		public static Graph<ConcreteNodeInfo, ConcreteEdgeInfo>.Node GetNodeByPos(Graph<ConcreteNodeInfo, ConcreteEdgeInfo> graph, int x, int y, int width)
		{
			 return graph.GetNode(GetNodeIdFromPos(x, y,width));
		}
		
		public static int GetNodeIdFromPos(int left, int top, int width)
		{
			return top * width + left;
		}

		private static void AddEdge(Graph<ConcreteNodeInfo, ConcreteEdgeInfo> graph, int nodeId, int x, int y, int width, int height, bool isDiag = false)
		{
			if (y < 0 || y >= height || x < 0 || x >= width)
				return;

			var targetNode = GetNodeByPos(graph, x, y, width);
			var cost = targetNode.Info.Cost;
			cost = isDiag ? (cost * 34) / 24 : cost;
			graph.AddEdge(nodeId, targetNode.NodeId, new ConcreteEdgeInfo(cost));
		}

		private static void CreateEdges(Graph<ConcreteNodeInfo, ConcreteEdgeInfo> graph, int width, int height, TileType tileType)
		{
			for (var top = 0; top < height; ++top)
				for (var left = 0; left < width; ++left)
				{
					var nodeId = GetNodeByPos(graph, left, top, width).NodeId;

					AddEdge(graph, nodeId, left, top - 1, width, height);
					AddEdge(graph, nodeId, left, top + 1, width, height);
					AddEdge(graph, nodeId, left - 1, top, width, height);
					AddEdge(graph, nodeId, left + 1, top, width, height);
					if (tileType == TileType.Octile)
					{
						AddEdge(graph, nodeId, left + 1, top + 1, width, height, true);
						AddEdge(graph, nodeId, left - 1, top + 1, width, height, true);
						AddEdge(graph, nodeId, left + 1, top - 1, width, height, true);
						AddEdge(graph, nodeId, left - 1, top - 1, width, height, true);
					}
					else if (tileType == TileType.OctileUnicost)
					{
						AddEdge(graph, nodeId, left + 1, top + 1, width, height);
						AddEdge(graph, nodeId, left - 1, top + 1, width, height);
						AddEdge(graph, nodeId, left + 1, top - 1, width, height);
						AddEdge(graph, nodeId, left - 1, top - 1, width, height);
					}
					else if (tileType == TileType.Hex)
					{
						if (left % 2 == 0)
						{
							AddEdge(graph, nodeId, left + 1, top - 1, width, height);
							AddEdge(graph, nodeId, left - 1, top - 1, width, height);
						}
						else
						{
							AddEdge(graph, nodeId, left + 1, top + 1, width, height);
							AddEdge(graph, nodeId, left - 1, top + 1, width, height);
						}
					}
				}
		}

		private static void CreateNodes(int width, int height, Graph<ConcreteNodeInfo, ConcreteEdgeInfo> graph, IPassability passability)
		{
			for (var top = 0; top < height; ++top)
				for (var left = 0; left < width; ++left)
				{
					var nodeId = GetNodeIdFromPos(left, top, width);
					var position = new Position(left, top);
					int movementCost;
					var isObstacle = !passability.CanEnter(position, out movementCost);
					var info = new ConcreteNodeInfo(isObstacle, movementCost, new Position(left, top));

					graph.AddNode(nodeId, info);
				}
		}
	}
}
