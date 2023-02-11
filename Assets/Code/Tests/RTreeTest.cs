using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Growth.Util;
using Growth.Voronoi;
using System.Linq;

public class RTreeTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void RTreeTestAdd()
    {
        RTree<Vec3> tree = new RTree<Vec3>();

        Assert.AreEqual(0, tree.Count());
        Assert.IsTrue(tree.IsValid());
        Assert.IsTrue(tree.GetBounds().IsEmpty);

        tree.Insert(new Vec3(0, 0, 0));

        Assert.AreEqual(1, tree.Count());
        Assert.IsTrue(tree.IsValid());
        Assert.AreEqual(new VBounds(new Vec3(0, 0, 0), new Vec3(0, 0, 0)), tree.GetBounds());

        tree.Insert(new Vec3(1, 0, 0));

        Assert.AreEqual(2, tree.Count());
        Assert.IsTrue(tree.IsValid());
        Assert.AreEqual(new VBounds(new Vec3(0, 0, 0), new Vec3(1, 0, 0)), tree.GetBounds());

        tree.Insert(new Vec3(0, 1, 0));

        Assert.AreEqual(3, tree.Count());
        Assert.IsTrue(tree.IsValid());
        Assert.AreEqual(new VBounds(new Vec3(0, 0, 0), new Vec3(1, 1, 0)), tree.GetBounds());
    }

    [Test]
    public void RTreeTestAddRemoveLots()
    {
        RTree<Vec3> tree = new RTree<Vec3>();
        ClRand rand = new ClRand(3);

        for(int i = 0; i < 100; i++)
        {
            tree.Insert(rand.Vec3());
            Assert.IsTrue(tree.IsValid());
        }

        List<Vec3> all_nodes = tree.ToList();

        while(all_nodes.Count > 0)
        {
            int idx = rand.IntRange(0, all_nodes.Count);
            Vec3 v = all_nodes[idx];
            all_nodes.RemoveAt(idx);
            tree.Remove(v);
            Assert.IsTrue(tree.IsValid());
            Assert.AreEqual(all_nodes.Count, tree.Count());
        }
    }

    //// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    //// `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator RTreeTestWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
