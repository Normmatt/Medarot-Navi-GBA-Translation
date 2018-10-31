using System;
using System.Collections.Generic;
using System.Text;

namespace Nintenlord.Collections
{
    enum NodeMode
    {
        Undefined,
        Item, 
        Branch
    }

    class Octree<T>
    {
        OctreeNode top;
        int maxDepth;
        int minDepth;

        public Octree(int minDepth, int maxDepth)
        {
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
            top = new Octree<T>.OctreeNode();
        }

        public int AmountOfItems()
        {
            return AmountOfLeaves(top);
        }

        private int AmountOfLeaves(OctreeNode node)
        {
            if (node == null)
                return 0;
            else if (node.item != null)
                return 1;
            else
            {
                int result = 0;
                for (int i = 0; i < node.amountOfNodes; i++)
                    result += AmountOfLeaves(node[i]);
                return result;
            }
        }
        
        public void AddItem(T item, int[] position)
        {
            if (position.Length < minDepth)
                return;

            OctreeNode currentPosition = top;
            int length = Math.Min(position.Length, maxDepth) - 1;
            for (int i = 0; i < length; i++)
            {
                if (currentPosition[position[i]] == null)
                {
                        currentPosition[position[i]] = new Octree<T>.OctreeNode();
                }
                currentPosition = currentPosition[position[i]];
            }
            currentPosition[position[length]] = new Octree<T>.OctreeNode(item);
        }

        public T GetItem(int[] position)
        {
            if (position.Length < minDepth)
                return default(T);

            OctreeNode currentPosition = top;
            int length = Math.Min(position.Length, maxDepth) - 1;
            for (int i = 0; i < length; i++)
            {
                if (currentPosition[position[i]] == null)
                    return default(T);
                currentPosition = currentPosition[position[i]];
            }
            return currentPosition[position[length]].item;
        }

        public T[] ToArray()
        {
            List<T> list = new List<T>();
            GetItems(top, list);
            return list.ToArray();
        }

        private void GetItems(OctreeNode node, ICollection<T> collection)
        {
            if (node.item != null)
                collection.Add(node.item);
            else
            {
                for (int i = 0; i < node.amountOfNodes; i++)
                {
                    if (node[i] != null)
                        GetItems(node[i], collection);
                }
            }
        }

        private class OctreeNode
        {
            OctreeNode[] nodes;
            public OctreeNode this[int x]
            {
                get 
                {
                    if (x < 8)
                        return nodes[x];
                    else
                        return null; 
                }
                set
                {
                    if (x < 8 && x >= 0)
                        nodes[x] = value;
                }
            }
            public int amountOfNodes
            {
                get { return nodes.Length; }
            }

            public T item
            {
                get;
                private set;
            }

            public OctreeNode(T item)
            {
                this.item = item;
            }

            public OctreeNode()
            {
                nodes = new OctreeNode[8];
            }
        }

    }
}
