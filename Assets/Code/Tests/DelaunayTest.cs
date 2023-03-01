using Growth.Util;
using Growth.Voronoi;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class DelaunayTest
{
    [Test]
    public void ConvexityTest()
    {
        var random = new ClRand(0);

        for (int i = 0; i < 10; i++)
        {
            var del = new Delaunay(1e-3f);
            var verts = new List<Vec3>();

            for (int j = 0; j < 10; j++)
            {
                var vert = random.Vec3() * 10 - new Vec3(5, 5, 5);

                verts.Add(vert);
            }

            var initial_tet = del.GetBoundingTet(verts);

            del.InitialiseWithTet(initial_tet.Verts.ToArray());

            Assert.IsTrue(del.Validate());
            Assert.IsTrue(IsConvex(del));

            for (int j = 0; j < 10; j++)
            {
                del.AddVert(verts[j]);

                Assert.IsTrue(del.Validate());
                Assert.IsTrue(IsConvex(del));
            }
        }
    }

    // this doesn't seem to hold, in that after we delete the outer construction-only tetrahedron, the inner shape need not be wholly convex...
    //[Test]
    //public void Convexity2Test()
    //{
    //    var random = new ClRand(0);

    //    for (int i = 0; i < 10; i++)
    //    {
    //        var del = new Delaunay(1e-3f);
    //        var verts = new List<Vec3>();

    //        for (int j = 0; j < 10; j++)
    //        {
    //            var vert = random.Vec3() * 10 - new Vec3(5, 5, 5);

    //            verts.Add(vert);
    //        }

    //        del.InitialiseWithVerts(verts);

    //        Assert.IsTrue(del.Validate());
    //        Assert.IsTrue(IsConvex(del));
    //    }
    //}

    private bool IsConvex(IDelaunay del)
    {
        foreach(var face in del.OuterSurface().Faces)
        {
            foreach(var vert in del.Verts.Where(vert => !face.Verts.Contains(vert)))
            {
                if (!face.IsVertInside(vert, 1e-4f))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
