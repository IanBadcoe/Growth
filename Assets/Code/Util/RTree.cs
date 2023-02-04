using Growth.Voronoi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Growth.Util
{
    public interface IBounded
    {
        VBounds GetBounds();
    }

    public class RTree<T> where T : class, IBounded
    {
        const int MaxChildren = 7;
        const int MinChildren = (MaxChildren + 1) / 2;

        class Node
        {
            public Node Parent;
            public List<Node> Children = null;
            public T Item;
            VBounds Bound = new VBounds();

            public bool IsDirty = false;                       // when Bound needs recalculation

            public Node()
            {
                // empty tree
            }

            public Node(T item)
            {
                Item = item;
                IsDirty = true;
            }

            public Node(List<Node> nodes)
            {
                Children = nodes;

                foreach(var child in Children)
                {
                    child.Parent = this;
                }

                IsDirty = true;
            }

            public VBounds GetBound()
            {
                if (IsDirty)
                {
                    RecalculateBound();
                }

                return Bound;
            }

            private void RecalculateBound()
            {
                if (Item != null)
                {
                    Bound = Item.GetBounds();
                }
                else if (Children != null)
                {
                    Bound = Children.Aggregate(new VBounds(), (b, c) => b.UnionedWith(c.GetBound()));
                }
                else
                {
                    Bound = new VBounds();
                }

                IsDirty = false;
            }

            public bool IsValid => Item == null || Children.Count == 0;

            public bool IsEmpty => Children == null && Item == null;

            public bool IsLeaf => Children == null;
        }

        Node Root = new Node();

        public RTree()
        {
            // nothing, initialisers give us an empty tree
        }

        public void Insert(T item)
        {
            if (Root.IsEmpty)
            {
                Root.Item = item;
                Root.IsDirty = true;
                return;
            }

            var item_node = new Node(item);

            if (Root.IsLeaf)
            {
                Root.Children = new List<Node> { item_node, new Node(Root.Item) };
                Root.Item = null;
                Root.IsDirty = true;
            }
            else
            {
                var new_node = InsertNode(Root, item_node);

                if (new_node != null)
                {
                    Root = new Node(new List<Node> { Root, new_node });
                }
            }
        }

        private Node InsertNode(Node node, Node insert_leaf_node)
        {
            Node insert_here_node;

            if (!node.Children[0].IsLeaf)
            {
                // we reached the penultimate level, insert into this node
                insert_here_node = insert_leaf_node;
            }
            else
            {
                // recurse into children and get any (split) node to insert here back
                var insert_node = ChooseNode(node.Children, insert_leaf_node);

                insert_here_node = InsertNode(insert_node, insert_leaf_node);
            }

            if (insert_here_node == null)
                return null;

            if (node.Children.Count < MaxChildren)
            {
                node.Children.Add(insert_here_node);
                insert_here_node.Parent = node;
                node.IsDirty = true;

                return null;
            }

            return SplitNode(node, insert_here_node);
        }

        private Node SplitNode(Node node, Node new_node)
        {
            var all_nodes = node.Children.Append(new_node).ToList();

            Tuple<Node, Node> extremes = FindExtremes(all_nodes);

            List<Node> set1 = new List<Node> { extremes.Item1 };
            List<Node> set2 = new List<Node> { extremes.Item2 };

            VBounds bound1 = extremes.Item1.GetBound();
            VBounds bound2 = extremes.Item2.GetBound();

            float volume1 = bound1.Volume + 1;
            float volume2 = bound2.Volume + 1;

            foreach (var child in all_nodes.Where(n => n != extremes.Item1 && n != extremes.Item2))
            {
                if (set1.Count == MinChildren)
                {
                    set2.Add(child);
                }
                else if (set2.Count == MinChildren)
                {
                    set1.Add(child);
                }
                else
                {
                    VBounds new_bound1 = bound1.UnionedWith(child.GetBound());
                    VBounds new_bound2 = bound2.UnionedWith(child.GetBound());

                    float new_volume1 = new_bound1.Volume + 1;
                    float new_volume2 = new_bound2.Volume + 1;

                    bool choose1 = false;

                    // if adding it to set 1 increases the total volume by less than adding it to set 2
                    if (new_volume1 + volume2 < new_volume2 + volume1)
                    {
                        choose1 = true;
                    }
                    // if adding it to set 1 increases the total volume by mode than adding it to set 2
                    else if (new_volume1 + volume2 > new_volume2 + volume1)
                    {
                        choose1 = false;
                    }
                    // otherwise if the change in volume is the same, go for the one which leads to the smaller new volume
                    else if (new_volume1 < new_volume2)
                    {
                        choose1 = true;
                    }
                    // and if that was a draw too, we do not care...
                    else
                    {
                        choose1 = false;
                    }

                    if (choose1)
                    {
                        set1.Add(child);
                        bound1 = new_bound1;
                        volume1 = new_volume1;
                    }
                    else
                    {
                        set2.Add(child);
                        bound2 = new_bound2;
                        volume2 = new_volume2;
                    }
                }
            }

            // put half the children in the original node and return a node containing the other half for insertion into
            // our parent
            node.Children = set1;
            node.IsDirty = true;

            return new Node(set2);
        }

        private Tuple<Node, Node> FindExtremes(List<Node> nodes)
        {
            // max_x will be the highest, _minimum_ x on a bound
            // and min_x will be the lowest _maximum_ x
            // and that finds the bounds with the highest separation
            // (and cannot find the same bound, twice)

            float max_x = float.MinValue;
            float max_y = float.MinValue;
            float max_z = float.MinValue;

            float min_x = float.MaxValue;
            float min_y = float.MaxValue;
            float min_z = float.MaxValue;

            Node max_x_node = null;
            Node max_y_node = null;
            Node max_z_node = null;

            Node min_x_node = null;
            Node min_y_node = null;
            Node min_z_node = null;

            foreach (var node in nodes)
            {
                VBounds b = node.GetBound();

                if (max_x > b.Min.X)
                {
                    max_x = b.Min.X;
                    max_x_node = node;
                }

                if (max_y > b.Min.Y)
                {
                    max_y = b.Min.Y;
                    max_y_node = node;
                }

                if (max_z > b.Min.Z)
                {
                    max_z = b.Min.Z;
                    max_z_node = node;
                }

                if (min_x < b.Max.X)
                {
                    min_x = b.Max.X;
                    min_x_node = node;
                }

                if (min_y < b.Max.Y)
                {
                    min_y = b.Max.Y;
                    min_y_node = node;
                }

                if (min_z < b.Max.Z)
                {
                    min_z = b.Max.Z;
                    min_z_node = node;
                }
            }

            var ret = new Tuple<Node, Node>(min_x_node, max_x_node);
            var range = max_x - min_x;

            if (max_y - min_y > range)
            {
                ret = new Tuple<Node, Node>(min_y_node, max_y_node);
                range = max_y - min_y;
            }

            if (max_z - min_z > range)
            {
                ret = new Tuple<Node, Node>(min_z_node, max_z_node);
            }

            return ret;
        }

        private Node ChooseNode(List<Node> children, Node insert_leaf_node)
        {
            Node chosen = children[0];
            float chosen_growth_ratio = 1;
            float chosen_volume = 0;

            // the +1s allow us to divide, even if the original volume was zero
            // without changing which ratios are bigger that each other...
            float new_volume = chosen.GetBound().UnionedWith(insert_leaf_node.GetBound()).Volume + 1;
            float old_volume = chosen.GetBound().Volume + 1;

            chosen_growth_ratio = new_volume / old_volume;
            if (chosen_growth_ratio == 1)
            {
                chosen_volume = new_volume;
            }

            foreach (var child in children.Skip(1))
            {
                new_volume = child.GetBound().UnionedWith(insert_leaf_node.GetBound()).Volume + 1;

                if (chosen_volume != 0 && new_volume < chosen_volume)
                {
                    chosen_volume = new_volume;
                    chosen = child;
                    continue;
                }

                old_volume = child.GetBound().Volume + 1;

                float growth_ratio = new_volume / old_volume;
                if (growth_ratio < chosen_growth_ratio)
                {
                    chosen_growth_ratio = growth_ratio;
                    chosen = child;

                    if (chosen_growth_ratio == 1)
                    {
                        chosen_volume = new_volume;
                    }
                }
            }

            return chosen;
        }
    }
}
