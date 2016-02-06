﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TspLibNet;
using TspLibNet.Graph.Nodes;

namespace AntSimComplexTspLibItemManager.Utilities
{
  /// <summary>
  /// This class is a wrapper for TspLib95Item objects representing symmetric TSP problems
  /// that fit the research criteria of a hundred nodes or less and with 2D node coordinates.
  /// </summary>
  internal class SymmetricTspItemInfoProvider
  {
    /// <returns>The name of the current TSP problem.</returns>
    public string ProblemName { get; }

    /// <returns>The maximum "x" coordinate of all the nodes in the graph</returns>
    public double MaxXCoordinate { get; }

    /// <returns>The minimum "x" coordinate of all the nodes in the graph</returns>
    public double MinXCoordinate { get; }

    /// <returns>The maximum "y" coordinate of all the nodes in the graph</returns>
    public double MaxYCoordinate { get; }

    /// <returns>The minimum "y" coordinate of all the nodes in the graph</returns>
    public double MinYCoordinate { get; }

    public bool HasOptimalTour { get; }

    /// <returns>The optimal tour length if known, double.MaxValue if not.</returns>
    public double OptimalTourLength { get; } = double.MaxValue;

    /// <returns>A list of TspNode objects corresponding to the optimal tour for the problem (if it is known).</returns>
    public List<TspNode> OptimalTour { get; } = new List<TspNode>();

    /// <returns>A list of Points corresponding to the current nodes' coordinates.</returns>
    public IEnumerable<Point> NodeCoordinatesAsPoints { get; }

    /// <summary>
    /// INode ID's aren't necessarily zero-based.  This integer keeps track of the difference between
    /// the INode ID's and the zero-based indices used everywhere else.
    /// </summary>
    private readonly int _zeroBasedIdOffset;

    private List<TspNode> TspNodes { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="item">The item to provide information for (must be symmetric TSP item with 2D nodes).</param>
    /// <exception cref="ArgumentNullException">Thrown when "item" is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if problem nodes are not Node2D types.</exception>
    public SymmetricTspItemInfoProvider(TspLib95Item item)
    {
      if (item == null)
      {
        throw new ArgumentNullException(nameof(item));
      }

      var nodes2D = item.Problem.NodeProvider.GetNodes();
      TspNodes = nodes2D.OfType<Node2D>().Select(n => new TspNode(n.Id, n.X, n.Y)).ToList();
      if (TspNodes.Any() == false)
      {
        string errMsg = $"Selected problem: {item.Problem.Name} does not contain Node2D objects.";
        throw new ArgumentOutOfRangeException(nameof(item), errMsg);
      }

      ProblemName = item.Problem.Name;

      _zeroBasedIdOffset = TspNodes.Min(n => n.Id) - 0;

      MaxXCoordinate = TspNodes.Max(i => i.X);
      MinXCoordinate = TspNodes.Min(i => i.X);
      MaxYCoordinate = TspNodes.Max(i => i.Y);
      MinYCoordinate = TspNodes.Min(i => i.Y);
      NodeCoordinatesAsPoints = TspNodes.Select(n => new Point { X = n.X, Y = n.Y });

      if (item.OptimalTour != null)
      {
        var nodes = item.OptimalTour.Nodes.Select(n => item.Problem.NodeProvider.GetNode(n));
        OptimalTour = nodes.OfType<Node2D>().Select(n => new TspNode(n.Id, n.X, n.Y)).ToList();
        OptimalTourLength = item.OptimalTourDistance;
        HasOptimalTour = true;
      }
    }

    /// <summary>
    /// Constructs a tour consisting of TspNode elements from the zero based tour indices
    /// representing an Ant's tour of the TSP graph.
    /// </summary>
    /// <param name="antTourIndices">A list of zero-based node indices.</param>
    /// <returns>A list of TspNode objects representing an Ant's constructed tour.</returns>
    public IEnumerable<TspNode> BuildNode2DTourFromZeroBasedIndices(IEnumerable<int> antTourIndices)
    {
      return antTourIndices.Select(index => TspNodes.First(n => n.Id == index + _zeroBasedIdOffset));
    }
  }
}