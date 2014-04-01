﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Catrobat.Paint.Phone.Command;

namespace Catrobat.Paint.Phone.Tool
{
    class LineTool : ToolBase
    {
        private Path _path;
        private PathGeometry _pathGeometry;
        private PathFigureCollection _pathFigureCollection;
        private PathFigure _pathFigure;
        private PathSegmentCollection _pathSegmentCollection;
        private Point _lastPoint;
        private bool _lastPointSet;
        private LineSegment _lineSegment;

        public LineTool()
        {
            this.ToolType = ToolType.Line;

        }

        public override void HandleDown(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }

            var coordinate = (Point)arg;

            _path = new Path();
            _pathGeometry = new PathGeometry();
            _path.StrokeLineJoin = PenLineJoin.Bevel;

            _path.Stroke = PocketPaintApplication.GetInstance().PaintData.ColorSelected;
            _path.StrokeThickness = PocketPaintApplication.GetInstance().PaintData.ThicknessSelected;
            _path.StrokeEndLineCap = PocketPaintApplication.GetInstance().PaintData.CapSelected;
            _path.StrokeStartLineCap = PocketPaintApplication.GetInstance().PaintData.CapSelected;

            _path.Data = _pathGeometry;
            _pathFigureCollection = new PathFigureCollection();
            _pathGeometry.Figures = _pathFigureCollection;
            _pathFigure = new PathFigure();

            _pathFigureCollection.Add(_pathFigure);
            _lastPoint = coordinate;
            _pathFigure.StartPoint = coordinate;
            _pathSegmentCollection = new PathSegmentCollection();
            _pathFigure.Segments = _pathSegmentCollection;

            PocketPaintApplication.GetInstance().PaintingAreaCanvas.Children.Add(_path);

            //            var transform = PocketPaintApplication.GetInstance().PaintingAreaCanvas.TransformToVisual(PocketPaintApplication.GetInstance().PaintingAreaLayoutRoot);
            //            var absolutePosition = transform.Transform(new Point(0, 0));
            var r = new RectangleGeometry
            {
                Rect = new Rect(0, 0, PocketPaintApplication.GetInstance().PaintingAreaCanvas.ActualWidth,
                    PocketPaintApplication.GetInstance().PaintingAreaCanvas.ActualHeight)
            };
            _path.Clip = r;
            _path.InvalidateArrange();
            _path.InvalidateMeasure();
            _lineSegment = new LineSegment();

        }

        public override void HandleMove(object arg)
        {
            if (!(arg is Point))
            {
                return;
            }

            var coordinate = (Point)arg;
            _pathSegmentCollection.Remove(_lineSegment);
            _lineSegment.Point = coordinate;
            _pathSegmentCollection.Add(_lineSegment);

            PocketPaintApplication.GetInstance().PaintingAreaLayoutRoot.InvalidateMeasure();
            _path.InvalidateArrange();
        }

        public override void HandleUp(object arg)
        {
          
        }

        public override void Draw(object o)
        {
            
        }
    }
}