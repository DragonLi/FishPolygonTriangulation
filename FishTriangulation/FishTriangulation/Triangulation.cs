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

    public class TriangleIndex
    {
        public int a, b, c;
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
            
            lowest = null;//simple way to must sure new lowest is computed again, don't check whether the lowest is changed
            validCount--;
            for (int i = vertex; i < validCount; i++)
            {
                Points[i] = Points[i + 1];
            }
        }

        private bool IsConvex(TriangleIndex tri)
        {
            //if (OrientCCW(middleVertex) * OrientCCW(LowestVertex()) > 0)
            //not xor is same as above with lower cpu & mem footprint
            if (!(OrientCCW(tri) ^ LowestCCWOrient()))
            {
                return true;
            }

            return false;
        }

        private bool OrientCCW(TriangleIndex tri)
        {
            return ccw(tri.a, tri.b, tri.c) > 0;
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

        private TriangleIndex lowest;
        private bool lowestCCW;

        private bool LowestCCWOrient()
        {
            if (lowest != null)
                return lowestCCW;
            lowestCCW = OrientCCW(LowestTri());
            return lowestCCW;
        }

        private TriangleIndex LowestTri()
        {
            if (lowest != null)
                return lowest;
            var lowestIndex = LowestVertex();
            lowest=GetTriangleIndex(lowestIndex);
            return lowest;
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

        private bool IsEmpty(TriangleIndex tri)
        {
            var previous = tri.a;
            var middleVertex = tri.b;
            var next = tri.c;
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

        private TriangleIndex GetTriangleIndex(int middleVertex)
        {
            var previous = middleVertex==0 ? validCount - 1 : middleVertex - 1;
            var next = middleVertex==validCount-1 ? 0 : middleVertex + 1;
            var result = new TriangleIndex {a = previous, b = middleVertex, c = next};
            return result;
        }

        private Triangle GetTriangleAt(TriangleIndex tri)
        {
            var result = new Triangle {a = Points[tri.a], b = Points[tri.b], c = Points[tri.c]};
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
                    var trInd = GetTriangleIndex(j);
                    if (IsConvex(trInd) && IsEmpty(trInd))
                    {
                        var t = GetTriangleAt(trInd);
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