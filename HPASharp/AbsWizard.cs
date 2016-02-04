﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPASharp
{
    public enum EntranceStyle
    {
        MIDDLE_ENTRANCE, END_ENTRANCE
    }
    

    public class AbsWizard
    {
        const int MAX_ENTRANCE_WIDTH = 6;

        public HTiling AbsTiling { get; set; }
        public Tiling Tiling { get; set; }
        public EntranceStyle EntranceStyle { get; set; }
        public int AbstractionRate { get; set; }
        public int ClusterSize { get; set; }

        public AbsWizard(Tiling tiling, int clusterSize, int maxLevel, EntranceStyle style)
        {
            this.ClusterSize = clusterSize;
            this.EntranceStyle = style;
            Tiling = new Tiling(tiling, 0, 0, tiling.Width, tiling.Height);
            AbsTiling = new HTiling(clusterSize, maxLevel, tiling.Height, tiling.Width);
        }

        public void AbstractMaze()
        {
            CreateEntrancesAndClusters();
            CreateAbstractGraph();
        }

        private void CreateEntrancesAndClusters()
        {
            // now build clusters
            int row = 0, col = 0, clusterId = 0;
            int entranceId = 0;
            int horizSize, vertSize;

            //cerr << "Creating entrances and clusters...\n";
            AbsTiling.SetType(Tiling.TileType);
            for (int j = 0; j < Tiling.Height; j+= ClusterSize)
            {
                col = 0;
                for (int i = 0; i < Tiling.Width; i+= ClusterSize)
                {
                    horizSize = Math.Min(ClusterSize, Tiling.Width - i);
                    vertSize = Math.Min(ClusterSize, Tiling.Height - j);
                    var cluster = new Cluster(Tiling, clusterId++, row, col, new Position(i, j), new Size(horizSize, vertSize));
                    AbsTiling.addCluster(cluster);

                    // add entrances
                    if (j > 0 && j < Tiling.Height)
                    {
                        int lastId;
                        CreateHorizEntrances(i, i + horizSize - 1, j - 1, GetClusterId(row - 1, col), GetClusterId(row, col), entranceId, out lastId);
                        entranceId = lastId;
                    }
                    if (i > 0 && i < Tiling.Width)
                    {
                        int lastId;
                        createVertEntrances(j, j + vertSize - 1, i - 1, GetClusterId(row, col - 1), GetClusterId(row, col), entranceId, out lastId);
                        entranceId = lastId;
                    }
                    //             if (m_absTiling.getType() == AbsTiling::ABSTRACT_OCTILE)
        //             {
        //                 if (j > 0 && j < m_tiling.getHeight())
        //                     createDHEntrances(i, i + horizSize - 2, j - 1, row - 1, col, &entranceId);
        //                 if(i > 0 && i < m_tiling.getWidth())
        //                     createDVEntrances(j, j + vertSize - 2, i - 1, row, col - 1, &entranceId);
        //             }
                    col++;
                }
                row++;
            }
            // set the abstract size of the abstract tiling (e.g., # of cluster rows & cols)
        //     m_absTiling.setRows(row);
        //     m_absTiling.setColumns(col);
            AbsTiling.addAbstractNodes();
            AbsTiling.computeClusterPaths();
        }

        /// <summary>
        /// Gets the cluster Id, determined by its row and column
        /// </summary>
        public int GetClusterId(int row, int col)
        {
            int cols = (Tiling.Columns / ClusterSize);
            if (Tiling.Columns % ClusterSize > 0)
                cols++;
            return row * cols + col;
        }

        private void CreateAbstractGraph()
        {
            AbsTiling.createGraph();
        }

        // TODO: Together with Vert Entrances, refactor the code, they are too similar!
        private void CreateHorizEntrances(
            int start,
            int end,
            int latitude,
            int clusterid1,
            int clusterid2,
            int currId,
            out int lastId)
        {
            int node1Id, node2Id;
            var curreIdCounter = currId;

            // rolls over the horitzontal edge between start and end in order to find edges between
            // the top cluster (latitude marks the other cluster entrance line)
            for (int i = start; i <= end; i++)
            {
                node1Id = Tiling.getNodeId(latitude, i);
                node2Id = Tiling.getNodeId(latitude + 1, i);
                var node1isObstacle = Tiling.Graph.GetNodeInfo(node1Id).IsObstacle;
                var node2isObstacle = Tiling.Graph.GetNodeInfo(node2Id).IsObstacle;
                // get the next communication spot
                if (node1isObstacle || node2isObstacle)
                {
                    continue;
                }
                // start building and tracking the entrance
                int entranceStart = i;
                while (true)
                {
                    i++;
                    if (i >= end)
                        break;
                    node1Id = Tiling.getNodeId(latitude, i);
                    node2Id = Tiling.getNodeId(latitude + 1, i);
                    node1isObstacle = Tiling.Graph.GetNodeInfo(node1Id).IsObstacle;
                    node2isObstacle = Tiling.Graph.GetNodeInfo(node2Id).IsObstacle;
                    if (node1isObstacle || node2isObstacle || i >= end)
                        break;
                }
                if (EntranceStyle == EntranceStyle.END_ENTRANCE && (i - entranceStart) > MAX_ENTRANCE_WIDTH)
                {
                    // If the tracked entrance is big, create 2 entrance points at the edges of the entrance.
                    // create two new entrances, one for each end
                    var entrance1 = new Entrance((curreIdCounter)++, clusterid1, clusterid2, latitude, entranceStart,
                                       this.Tiling.getNodeId(latitude, entranceStart),
                                       this.Tiling.getNodeId(latitude + 1, entranceStart), Orientation.HORIZONTAL);
                    AbsTiling.addEntrance(entrance1);
                    var entrance2 = new Entrance((curreIdCounter)++, clusterid1, clusterid2, latitude, (i - 1),
                                       this.Tiling.getNodeId(latitude, i - 1),
                                       this.Tiling.getNodeId(latitude + 1, i - 1), Orientation.HORIZONTAL);
                    AbsTiling.addEntrance(entrance2);
                }
                else
                {
                    // if it is small, create one entrance in the middle 
                    var entrance = new Entrance((curreIdCounter)++, clusterid1, clusterid2, latitude, ((i - 1) + entranceStart) / 2,
                                      this.Tiling.getNodeId(latitude, ((i - 1) + entranceStart) / 2),
                                      this.Tiling.getNodeId(latitude + 1, ((i - 1) + entranceStart) / 2), Orientation.HORIZONTAL);
                    AbsTiling.addEntrance(entrance);
                }
            }

            lastId = curreIdCounter;
        }

        private void createVertEntrances(int start, int end, int meridian, int clusterid1,
            int clusterid2, int currId, out int lastId)
        {
            int node1Id, node2Id;
            var curreIdCounter = currId;

            for (int i = start; i <= end; i++)
            {
                node1Id = Tiling.getNodeId(i, meridian);
                node2Id = Tiling.getNodeId(i, meridian + 1);
                var node1Info = Tiling.Graph.GetNodeInfo(node1Id);
                var node2Info = Tiling.Graph.GetNodeInfo(node2Id);
                // get the next communication spot
                if (node1Info.IsObstacle || node2Info.IsObstacle)
                {
                    continue;
                }
                // start building the entrance
                int entranceStart = i;
                while (true)
                {
                    i++;
                    if (i >= end)
                        break;
                    node1Id = Tiling.getNodeId(i, meridian);
                    node2Id = Tiling.getNodeId(i, meridian + 1);
                    node1Info = Tiling.Graph.GetNodeInfo(node1Id);
                    node2Info = Tiling.Graph.GetNodeInfo(node2Id);
                    if ((node1Info.IsObstacle || node2Info.IsObstacle) || i >= end)
                        break;
                }
                if (EntranceStyle == EntranceStyle.END_ENTRANCE && (i - entranceStart) > MAX_ENTRANCE_WIDTH)
                {
                    // create two entrances, one for each end
                    var entrance1 = new Entrance(curreIdCounter++, clusterid1, clusterid2, entranceStart, meridian,
                                       this.Tiling.getNodeId(entranceStart, meridian),
                                       this.Tiling.getNodeId(entranceStart, meridian + 1), Orientation.VERTICAL);
                    AbsTiling.addEntrance(entrance1);

                    // BEWARE! We are getting the tileNode for position i - 1. If clustersize was 8
                    // for example, and end would had finished at 7, you would set the entrance at 6.
                    // This seems to be intended.
                    var entrance2 = new Entrance(curreIdCounter++, clusterid1, clusterid2, (i - 1), meridian,
                                       this.Tiling.getNodeId(i - 1, meridian),
                                       this.Tiling.getNodeId(i - 1, meridian + 1), Orientation.VERTICAL);
                    AbsTiling.addEntrance(entrance2);
                }
                else
                {
                    // create one entrance
                    var entrance = new Entrance(curreIdCounter++, clusterid1, clusterid2, ((i - 1) + entranceStart) / 2, meridian,
                                      this.Tiling.getNodeId(((i - 1) + entranceStart) / 2, meridian),
                                      this.Tiling.getNodeId(((i - 1) + entranceStart) / 2, meridian + 1), Orientation.VERTICAL);
                    AbsTiling.addEntrance(entrance);
                }
            }

            lastId = curreIdCounter;
        }
    }
}
