﻿using AntSimComplexAlgorithms;
using AntSimComplexAlgorithms.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using TspLibNet;
using TspLibNet.Graph.Nodes;

namespace AntSimComplexUI.Utilities
{
  /// <summary>
  /// This class is responsible for selecting the symmetrical TSP problems that we're interested in.
  /// For the sake of this research application only problems with fewer than or equal to 100 nodes and
  /// 2D coordinate sets are considered.
  /// </summary>
  public class SymmetricTspInfoProvider
  {
    /// <returns>The name of the current TSP problem.</returns>
    public string ProblemName { get; }

    /// <returns>A list of Node2D objects corresponding to the optimal tour for the problem (if it is known).</returns>
    public List<Node2D> OptimalTourNodes2D { get; } = new List<Node2D>();

    /// <returns>The optimal tour length if known, double.MaxValue if not.</returns>
    public double OptimalTourLength { get; } = double.MaxValue;

    public bool HasOptimalTour { get; }

    /// <returns>A list of problem graph Node2D objects.</returns>
    private List<Node2D> Nodes2D { get; }

    /// <summary>
    /// INode ID's aren't necessarily zero-based.  This integer keeps track of the difference between
    /// the INode ID's and the zero-based indices used by <seealso cref="AntSimComplexAlgorithms"/>, the underlying
    /// <seealso cref="DataStructures"/> and <seealso cref="Ant"/>.
    /// </summary>
    private readonly int _zeroBasedOffset;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="item">The item to provide information for (must be symmetric TSP item with 2D nodes).</param>
    /// <exception cref="ArgumentNullException">Thrown when "item" is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if problem nodes are not Node2D types.</exception>
    public SymmetricTspInfoProvider(TspLib95Item item)
    {
      if (item == null)
      {
        throw new ArgumentNullException(nameof(item));
      }

      var nodes2D = item.Problem.NodeProvider.GetNodes();
      Nodes2D = nodes2D.OfType<Node2D>().ToList();
      if (Nodes2D.Any() == false)
      {
        string errMsg = $"Selected problem: {item.Problem.Name} does not contain Node2D objects.";
        throw new ArgumentOutOfRangeException(nameof(item), errMsg);
      }

      _zeroBasedOffset = Nodes2D.Min(n => n.Id) - 0;
      Debug.Assert(_zeroBasedOffset != 0);  //not sure if any of these problems have a non-zero ID base

      if (item.OptimalTour != null)
      {
        var nodes = item.OptimalTour.Nodes.Select(n => item.Problem.NodeProvider.GetNode(n));
        OptimalTourNodes2D = nodes.OfType<Node2D>().ToList();
        OptimalTourLength = item.OptimalTourDistance;
        HasOptimalTour = true;
      }

      ProblemName = item.Problem.Name;
    }

    /// <summary>
    /// Constructs a tour consisting of Node2D elements from the zero based tour indices
    /// representing an Ant's tour of the TSP graph.
    /// </summary>
    /// <param name="antTourIndices">A list of zero-based node indices.</param>
    /// <returns>A list of Node2D objects representing an Ant's constructed tour.</returns>
    public List<Node2D> BuildNode2DTourFromZeroBasedIndices(List<int> antTourIndices)
    {
      return antTourIndices.Select(index => Nodes2D.First(n => n.Id == index + _zeroBasedOffset)).ToList();
    }

    /// <returns>The maximum "x" coordinate of all the nodes in the graph</returns>
    public double GetMaxX()
    {
      return Nodes2D.Max(i => i.X);
    }

    /// <returns>The minimum "x" coordinate of all the nodes in the graph</returns>
    public double GetMinX()
    {
      return Nodes2D.Min(i => i.X);
    }

    /// <returns>The maximum "y" coordinate of all the nodes in the graph</returns>
    public double GetMaxY()
    {
      return Nodes2D.Max(i => i.Y);
    }

    /// <returns>The minimum "y" coordinate of all the nodes in the graph</returns>
    public double GetMinY()
    {
      return Nodes2D.Min(i => i.Y);
    }

    public IEnumerable<Point> GetPoints()
    {
      return Nodes2D.Select(n => new Point { X = n.X, Y = n.Y });
    }
  }
}