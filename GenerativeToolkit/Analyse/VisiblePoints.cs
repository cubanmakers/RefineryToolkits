﻿using Autodesk.DesignScript.Geometry;
using Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry;
using GenerativeToolkit.Graphs.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using graphs = GenerativeToolkit.Graphs.Graphs;

namespace Autodesk.GenerativeToolkit.Analyse
{
    public static class VisiblePoints
    {
        public static double VisibleFromOrigin(Point origin, List<Point> points, List<Polygon> boundary, List<Polygon> obstacles)
        {
            Polygon isovist = IsovistPolygon(boundary, obstacles, origin);
            gPolygon gPol = gPolygon.ByVertices(isovist.Points.Select(p => gVertex.ByCoordinates(p.X, p.Y, p.Z)).ToList());

            double totalPoints = points.Count;
            double visibilityAmount = 0;
 
            foreach (Point point in points)
            {
                gVertex vertex = gVertex.ByCoordinates(point.X, point.Y, point.Z);
                
                if (gPol.ContainsVertex(vertex))
                {
                    ++visibilityAmount;
                    //point.Dispose();
                }
            }
            isovist.Dispose();
            return (1/totalPoints) * visibilityAmount;
        }

        private static Polygon IsovistPolygon(List<Polygon> boundary, List<Polygon> obstacles, Point point)
        {
            BaseGraph baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, obstacles);

            if (baseGraph == null) { throw new ArgumentNullException("graph"); }
            if (point == null) { throw new ArgumentNullException("point"); }

            gVertex origin = gVertex.ByCoordinates(point.X, point.Y, point.Z);

            List<gVertex> vertices = graphs.VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
            List<Point> points = vertices.Select(v => Points.ToPoint(v)).ToList();
            // TODO: Implement better way of checking if polygon is self intersectingç

            Polygon polygon = Polygon.ByPoints(points);

            if (polygon.SelfIntersections().Length > 0)
            {
                points.Add(point);
                polygon = Polygon.ByPoints(points);
            }
            return polygon;
        }
    }
}
