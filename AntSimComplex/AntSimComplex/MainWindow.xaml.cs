﻿using AntSimComplex.Dialogs;
using AntSimComplex.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TspLibNet;
using TspLibNet.Graph.Nodes;

namespace AntSimComplex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int _circleWidth = 10;
        private const string _packageRelPath = @"..\..\..\packages";
        private const string _libPathRegistryKey = @"HKEY_CURRENT_USER\Software\AntSim\TSPLIB95Path";

        private string _tspLibPath;
        private TspLib95 _tspLib;
        private TspLibProcessor _tspLibProcessor;

        private Matrix _worldToCanvasMatrix;
        private Matrix _canvasToWorldMatrix;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialise();
        }

        private void Initialise()
        {
            BrowseTSPLIBPath();

            // Load all symmetric TSP instances.
            _tspLib = new TspLib95(_tspLibPath);
            _tspLib.LoadAllTSP();
            _tspLibProcessor = new TspLibProcessor(_tspLib);
            TSPCombo.ItemsSource = _tspLibProcessor.ProblemNames;
        }

        private void BrowseTSPLIBPath()
        {
            _tspLibPath = (string)Registry.GetValue(_libPathRegistryKey, "", "");

            var tspPathExists = Directory.Exists(_tspLibPath);
            var startDirectory = System.IO.Path.GetFullPath(_packageRelPath);

            do
            {
                var dialog = new DirectoryBrowserDialog(_tspLibPath, startDirectory);
                dialog.Owner = this;
                dialog.ShowDialog();

                _tspLibPath = dialog.DirectoryPath;
                tspPathExists = Directory.Exists(_tspLibPath);

                if (!tspPathExists)
                {
                    MessageBox.Show(this, "Path to TSPLIB95 is invalid.", "Error!");
                }
            } while (String.IsNullOrWhiteSpace(_tspLibPath) || !tspPathExists);

            Registry.SetValue(_libPathRegistryKey, "", _tspLibPath);
        }

        private void TSPCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var problemName = TSPCombo.SelectedItem.ToString();
            var worldMinX = _tspLibProcessor.GetMinX(problemName);
            var worldMinY = _tspLibProcessor.GetMinY(problemName);
            var worldMaxX = _tspLibProcessor.GetMaxX(problemName);
            var worldMaxY = _tspLibProcessor.GetMaxY(problemName);

            const double margin = 20;
            const double canvasMinX = margin;
            const double canvasMinY = margin;
            var canvasMaxX = canvas.ActualWidth - margin;
            var canvasMaxY = canvas.ActualHeight - margin;

            // Order of canvas Y min and max arguments are swapped due to canvas coordinate
            // system (top-left is 0,0).  This "flips" the coordinate system along the Y
            // axis by making the Y scale value negative so that we have bottom-left at 0,0.
            PrepareTransformationMatrices(worldMinX, worldMaxX, worldMinY, worldMaxY,
                                          canvasMinX, canvasMaxX, canvasMaxY, canvasMinY);

            canvas.Children.Clear();

            var nodes = _tspLibProcessor.GetNodes(problemName);
            var points = from n in nodes
                         select new Point { X = n.X, Y = n.Y };

            foreach (var point in points)
            {
                var ellipse = new Ellipse() { Width = _circleWidth, Height = _circleWidth, Fill = Brushes.Black };
                ellipse.ToolTip = $"x: {point.X}, y: {point.Y}";
                canvas.Children.Add(ellipse);
                var transformed = TransformWorldToCanvas(point);
                Canvas.SetLeft(ellipse, transformed.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, transformed.Y - ellipse.Height / 2);
            }
        }

        private void PrepareTransformationMatrices(
            double worldMinX, double worldMaxX, double worldMinY, double worldMaxY,
            double canvasMinX, double canvasMaxX, double canvasMinY, double canvasMaxY)
        {
            _worldToCanvasMatrix = Matrix.Identity;
            _worldToCanvasMatrix.Translate(-worldMinX, -worldMinY);

            double xscale = (canvasMaxX - canvasMinX) / (worldMaxX - worldMinX);
            double yscale = (canvasMaxY - canvasMinY) / (worldMaxY - worldMinY);
            _worldToCanvasMatrix.Scale(xscale, yscale);

            _worldToCanvasMatrix.Translate(canvasMinX, canvasMinY);

            _canvasToWorldMatrix = _worldToCanvasMatrix;
            _canvasToWorldMatrix.Invert();
        }

        private Point TransformWorldToCanvas(Point point)
        {
            return _worldToCanvasMatrix.Transform(point);
        }

        private Point TransformCanvasToWorld(Point point)
        {
            return _canvasToWorldMatrix.Transform(point);
        }
    }
}