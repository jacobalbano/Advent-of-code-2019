using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2019.Days
{
    class Day6 : DayBase
    {
        public override string Part1(string input)
        {
            return CountOrbits(input.ToLines()).ToString();
        }

        public override void Part1Test()
        {
            Assert.AreEqual(42, CountOrbits(new[] {
                "COM)B",
                "B)C",
                "C)D",
                "D)E",
                "E)F",
                "B)G",
                "G)H",
                "D)I",
                "E)J",
                "J)K",
                "K)L"
            }));
        }
        
        public override string Part2(string input)
        {
            return CountJumps("YOU", "SAN", input.ToLines()).ToString();
        }

        public override void Part2Test()
        {
            Assert.AreEqual(4, CountJumps("YOU", "SAN", new[] {
                    "COM)B",
                    "B)C",
                    "C)D",
                    "D)E",
                    "E)F",
                    "B)G",
                    "G)H",
                    "D)I",
                    "E)J",
                    "J)K",
                    "K)L",
                    "K)YOU",
                    "I)SAN"
            }));
        }
        
        private int CountOrbits(string[] input)
        {
            var totalOrbits = 0;
            var tree = TreeNode.Parse(input);
            tree.Walk((node, depth) => totalOrbits += depth);
            return totalOrbits;
        }

private int CountJumps(string from, string to, string[] input)
{
    var tree = TreeNode.Parse(input);

    TreeNode you = null, san = null;
    var depths = new Dictionary<TreeNode, int>();

    tree.Walk((node, depth) =>
    {
        if (node.Name == "YOU") you = node;
        if (node.Name == "SAN") san = node;
        depths[node] = depth;
    });

    var commonDepth = depths[FindCommonParent(you, san)];
    int youDepth = depths[you.Parent], sanDepth = depths[san.Parent];

    return (youDepth - commonDepth) + (sanDepth - commonDepth);

    TreeNode FindCommonParent(TreeNode a, TreeNode b)
    {
        if (a.Parent == b.Parent)
            return a.Parent;

        int aDepth = depths[a], bDepth = depths[b];
        if (aDepth < bDepth)
            return FindCommonParent(a, b.Parent);
        else if (aDepth > bDepth)
            return FindCommonParent(a.Parent, b);
        else
            return FindCommonParent(a.Parent, b.Parent);
    }
}

        private class TreeNode
        {
            public static TreeNode Parse(params string[] map)
            {
                var allNodes = new Dictionary<string, TreeNode>();
                foreach (var def in map)
                {
                    var parts = def.Split(')');
                    var parent = allNodes.GetOrCreate(parts[0], key => new TreeNode(key));
                    var child = allNodes.GetOrCreate(parts[1], key => new TreeNode(key));

                    if (child.Parent != null)
                        throw new Exception("Tried to re-assign a parent node!");
                    child.Parent = parent;
                    parent.internalChildren.Add(child);
                }

                return allNodes["COM"];
            }

            public TreeNode Parent { get; private set; }
            public string Name { get; }

            public IReadOnlyList<TreeNode> Children => internalChildren;

            private TreeNode(string name)
            {
                Name = name;
                internalChildren = new List<TreeNode>();
            }

            public void Walk(Walker walker)
            {
                Walk(walker, 0);
            }

            private void Walk(Walker walker, int depth)
            {
                walker(this, depth);
                foreach (var child in Children)
                    child.Walk(walker, depth + 1);
            }

            public delegate void Walker(TreeNode node, int depth);
            private List<TreeNode> internalChildren;
        }
    }
}
