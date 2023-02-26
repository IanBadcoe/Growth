using Growth.Util;
using Growth.Voronoi;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FaceTest
{
    [Test]
    public void EqualsTest()
    {
        var face1 = new Face(new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
        }, new Vec3(0, 0, 1));

        var face2 = new Face(new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
        }, new Vec3(0, 0, 1));

        Assert.IsTrue(face1.Equals(face2));
        Assert.IsTrue(face2.Equals(face1));

        Assert.IsTrue(face1.Reversed().Equals(face2.Reversed()));
        Assert.IsTrue(face1.Reversed().Reversed().Equals(face2));
        Assert.IsFalse(face1.Reversed().Equals(face1));

        var face3 = new Face(new List<Vec3>
        {
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
            new Vec3(0, 0, 0),
        }, new Vec3(0, 0, 1));

        Assert.IsTrue(face1.Equals(face3));

        var face4 = new Face(new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(0.5f, 0.1f, 0),
            new Vec3(0.5f, -0.1f, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
        }, new Vec3(0, 0, 1));

        var face4b = new Face(new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(0.5f, -0.1f, 0),
            new Vec3(0.5f, 0.1f, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
        }, new Vec3(0, 0, 1));

        Assert.IsFalse(face4.Equals(face4b));
    }

    [Test]
    public void HashTest()
    {
        var face1 = new Face(new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
        }, new Vec3(0, 0, 1));

        var face2 = new Face(new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
        }, new Vec3(0, 0, 1));

        Assert.IsTrue(face1.GetHashCode() == face2.GetHashCode());
        Assert.IsTrue(face2.GetHashCode() == face1.GetHashCode());

        Assert.IsTrue(face1.Reversed().GetHashCode() == face2.Reversed().GetHashCode());
        Assert.IsFalse(face1.Reversed().GetHashCode() == face1.GetHashCode());

        var face3 = new Face(new List<Vec3>
        {
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
            new Vec3(0, 0, 0),
        }, new Vec3(0, 0, 1));

        Assert.IsTrue(face1.GetHashCode() == face3.GetHashCode());

        var face4 = new Face(new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(0.5f, 0.1f, 0),
            new Vec3(0.5f, -0.2f, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
        }, new Vec3(0, 0, 1));

        var face4b = new Face(new List<Vec3>
        {
            new Vec3(0, 0, 0),
            new Vec3(0.5f, -0.2f, 0),
            new Vec3(0.5f, 0.1f, 0),
            new Vec3(1, 0, 0),
            new Vec3(1, 1, 0),
            new Vec3(0, 1, 0),
        }, new Vec3(0, 0, 1));

        Assert.IsFalse(face4.GetHashCode() == face4b.GetHashCode());
    }
}