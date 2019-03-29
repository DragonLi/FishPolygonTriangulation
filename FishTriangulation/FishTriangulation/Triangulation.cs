using System.Collections.Generic;

namespace FishTriangulation
{
    public struct Triangle
    {
        public Point2D a, b, c;
    }

    public struct Point2D
    {
        public float x, y;
    }

    public class Polygon2D //avoid name conflict of "polygon"
    {
        private List<Point2D> Points;

        public Polygon2D(List<Point2D> poly)
        {
            Points = poly;
            validCount = poly.Count;
        }
        
        private int validCount;

        private List<Triangle> CastToTriangle()
        {
            var r = new Triangle {a = Points[0], b = Points[1], c = Points[2]};
            var result = new List<Triangle>(1) {r};
            return result;
        }

        private void Prune(int vertex)
        {
            //skip check of vertex index < validCount for performance reason
            
            validCount--;
            for (int i = vertex; i < validCount; i++)
            {
                Points[i] = Points[i + 1];
            }
        }

        private bool IsConvex(int middleVertex)
        {
            //if (OrientCCW(middleVertex) * OrientCCW(LowestVertex()) > 0)
            //not xor is same as above with lower cpu & mem footprint
            if (!(OrientCCW(middleVertex) ^ OrientCCW(LowestVertex())))
            {
                return true;
            }

            return false;
        }

        private int LowestVertex()
        {
            //no cache of lowest vertex now, if has one, it must be deleted after prune

            //enlarge precision to avoid floating point problem!
            double minY = Points[0].y;
            int minyIndex = 0;
            for (int i = 1; i < validCount; i++)
            {
                if (Points[i].y < minY)
                {
                    minyIndex = i;
                    minY = Points[i].y;
                }
            }

            return minyIndex;
        }
        
        private bool OrientCCW(int middleVertex)
        {
            var previous = middleVertex==0 ? validCount - 1 : middleVertex - 1;
            var next = middleVertex==validCount-1 ? 0 : middleVertex + 1;
            return ccw(previous, middleVertex, next) > 0;
        }

        //sign of counter clockwise expression
        private int SignCCW(int previous, int middleVertex, int next)
        {
            var w = ccw(previous, middleVertex, next);
            if (w > 0)
            {
                return 1;
            }

            if (w < 0)
            {
                return -1;
            }

            return 0;
        }

        //counter clockwise expression, derive by cross product of vector and some underline logic is shown in https://algs4.cs.princeton.edu/91primitives/
        private double ccw(int previous, int middleVertex, int next)
        {
            return
                Points[previous].x * (Points[middleVertex].y - Points[next].y) +
                Points[middleVertex].x * (Points[next].y - Points[previous].y) +
                Points[next].x * (Points[previous].y - Points[middleVertex].y);
        }

        private bool IsEmpty(int middleVertex)
        {
            var previous = middleVertex==0 ? validCount - 1 : middleVertex - 1;
            var next = middleVertex==validCount-1 ? 0 : middleVertex + 1;
            var tsv = SignCCW(previous, middleVertex, next);
            for (int i = 0; i < validCount; i++)
            {
                if (i==previous || i==middleVertex || i==next)
                    continue;
                if (tsv * SignCCW(middleVertex, previous, i) >= 0 &&
                    tsv * SignCCW(previous, next, i) >= 0 &&
                    tsv * SignCCW(next, middleVertex, i) >= 0)
                {
                    return false;//vertex i is inside triangle with middle vertex at middleVertex 
                }
            }

            return true;
        }

        private Triangle GetTriangleAt(int middleVertex)
        {
            var previous = middleVertex==0 ? validCount - 1 : middleVertex - 1;
            var next = middleVertex==validCount-1 ? 0 : middleVertex + 1;
            var result = new Triangle {a = Points[previous], b = Points[middleVertex], c = Points[next]};
            return result;
        }
        
        public List<Triangle> EarClipping()
        {
            var totalVertex = Points.Count;
            if (totalVertex < 3)
                return null;//not a valid polygon
            if (totalVertex == 3)
                return CastToTriangle();//input is already a triangle


            var diagonals = totalVertex-3;
            var result = new List<Triangle>(diagonals);
            //cache Polygon2D.validCount
            var vertexCount = totalVertex;
            for (int i = 0; i < diagonals; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    var t = GetTriangleAt(j);
                    if (IsConvex(j) && IsEmpty(j))
                    {
                        result.Add(t);
                        vertexCount--;
                        Prune(j);
                        break;
                    }
                }
            }
            
            return result;
        }

    }
}