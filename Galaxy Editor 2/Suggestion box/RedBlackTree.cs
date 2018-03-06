using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Galaxy_Editor_2.Compiler.Contents;

namespace Galaxy_Editor_2.Suggestion_box
{
    class RedBlackTree<T> : ICollection<T> where T : class
    {
        private enum Color
        {
            Red, Black
        }
        private class Node
        {

            public Node p, r, l;
            public T element;
            public Color color = Color.Black;
            public int lSize = 0, rSize = 0;
            public int Size { get { return lSize + rSize + 1; } }

            public Node(Node l, Node r, Node p, T elm)
            {
                this.l = l;
                this.p = p;
                this.r = r;
                element = elm;
            }
        }

        public T this[int index]
        {
            get
            {
                
                Node x = root;
                int prevSize = 0;
                while (true)
                {
                    if (x == nil)
                        return null;
                    if (x.lSize + prevSize == index)
                        return x.element;
                    if (x.lSize + prevSize > index)
                        x = x.l;
                    else
                    {
                        prevSize += x.lSize + 1;
                        x = x.r;
                    }
                }
            }
        }



        public delegate int Compare(T elm1, T elm2);

        private readonly Compare comparere;
        private Node nil = new Node(null, null, null, null);
        private Node root;

        public RedBlackTree(Compare cmp)
        {
            comparere = cmp;
            Clear();
        }

        /*private void InvariantCheck()
        {
            int pathLength = 0;
            //The root must be black
            if (root.color != Color.Black)
                pathLength = pathLength;
            //Nil must be black
            if (nil.color != Color.Black)
                pathLength = pathLength;
            //Find a leaf
            Node x = root;
            while (x != nil)
            {
                if (x.color == Color.Black)
                    pathLength++;
                x = x.l;
            }
            InvariantCheck(root, pathLength, 0);
        }

        private void InvariantCheck(Node n, int pathLength, int curPathLength)
        {
            if (n == nil)
                return;
            //If a node is red, its children must be black
            if (n.color == Color.Red)
                if (n.l.color == Color.Red || n.r.color == Color.Red)
                    n = n;
            //All paths from root to leaf contain same number of black nodes
            if (n.color == Color.Black)
                curPathLength++;
            if (n.l == nil || n.r == nil)
                if (pathLength != curPathLength)
                    n = n;
            InvariantCheck(n.l, pathLength, curPathLength);
            InvariantCheck(n.r, pathLength, curPathLength);
        }*/

        #region rotate
        private void LeftRotate(Node x)
        {
            Node y = x.r;
            //Set x's right tree to y's left tree
            x.r = y.l;
            x.rSize = y.lSize;
            if (y.l != nil)
                y.l.p = x;
            //Set y's parent to x's parent
            y.p = x.p;
            if (x.p == nil)
                root = y;
            else if (x == x.p.l)
                x.p.l = y;
            else
                x.p.r = y;
            //Set y's left tree to x
            y.l = x;
            x.p = y;
            y.lSize = x.Size;
        }

        private void RightRotate(Node y)
        {
            Node x = y.l;
            //Set y's left child to x's right child
            y.l = x.r;
            y.lSize = x.rSize;
            if (x.r != nil)
                x.r.p = y;
            //Set x's parent to y's parent
            x.p = y.p;
            if (y.p == nil)
                root = x;
            else if (y.p.l == y)
                y.p.l = x;
            else
                y.p.r = x;
            //Set x's right tree to y
            x.r = y;
            y.p = x;
            x.rSize = y.Size;
        }
        #endregion

        public void Add(T item)
        {
            Node z = new Node(nil, nil, nil, item);
            Node y = nil;
            Node x = root;
            //Go down the tree untill you find a leaf where you insert the node
            while (x != nil)
            {
                y = x;
                //if z < x
                x = comparere(z.element, x.element) == -1 ? x.l : x.r;
            }
            //Dont want dublicates
            //if (y != nil && comparere(z.element, y.element) == 0)
            //    return;
            //Update parent/child pointers
            z.p = y;
            if (y == nil)
                root = z;
            else if (comparere(z.element, y.element) == -1)
                y.l = z;
            else
                y.r = z;
            z.l = nil;
            z.r = nil;
            z.color = Color.Red;
            AddFixup(z);
            Count++;
            SetSize(z, 1);
            //InvariantCheck();
            return;
        }

        private void SetSize(Node x, int mod)
        {
            while (x.p != nil)
            {
                if (x == x.p.l)
                    x.p.lSize += mod;
                else
                    x.p.rSize += mod;
                x = x.p;
            }
        }

        private void AddFixup(Node z)
        {
            while (z.p.color == Color.Red)
            {
                if (z.p == z.p.p.l)
                {
                    Node y = z.p.p.r;
                    if (y.color == Color.Red)
                    {
                        z.p.color = Color.Black;
                        y.color = Color.Black;
                        z.p.p.color = Color.Red;
                        z = z.p.p;
                    }
                    else
                    {
                        if (z == z.p.r)
                        {
                            z = z.p;
                            LeftRotate(z);
                        }
                        z.p.color = Color.Black;
                        z.p.p.color = Color.Red;
                        RightRotate(z.p.p);
                    }
                }
                else
                {
                    Node y = z.p.p.l;
                    if (y.color == Color.Red)
                    {
                        z.p.color = Color.Black;
                        y.color = Color.Black;
                        z.p.p.color = Color.Red;
                        z = z.p.p;
                    }
                    else
                    {
                        if (z == z.p.l)
                        {
                            z = z.p;
                            RightRotate(z);
                        }
                        z.p.color = Color.Black;
                        z.p.p.color = Color.Red;
                        LeftRotate(z.p.p);
                    }
                }
            }
            root.color = Color.Black;
        }

        public void Clear()
        {
            root = nil.l = nil.r = nil.p = nil;
            Count = 0;
        }

        public bool Contains(T item)
        {
            return Find(item) != nil;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            IEnumerator<T> enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                array[arrayIndex++] = enumerator.Current;
            }
        }

        public bool Remove(T item)
        {
            Node z = Find(item);
            if (z == nil)
                return false;
            Node y;
            if (z.l == nil || z.r == nil)
                y = z;
            else
                y = Successor(z);
            SetSize(y, -1);
            Node x;
            if (y.l != nil)
                x = y.l;
            else
                x = y.r;
            x.p = y.p;
            if (y.p == nil)
                root = x;
            else
                if (y == y.p.l)
                    y.p.l = x;
                else
                    y.p.r = x;
            if (y != z)
                z.element = y.element;
            //If z will contain keys, copy that too
            if (y.color == Color.Black)
                RemoveFixup(x);
            Count--;
            //InvariantCheck();
            return true;
        }

        private void RemoveFixup(Node x)
        {
            while (x != root && x.color == Color.Black)
            {
                if (x == x.p.l)
                {
                    Node w = x.p.r;
                    if (w.color == Color.Red)
                    {
                        w.color = Color.Black;
                        x.p.color = Color.Red;
                        LeftRotate(x.p);
                        w = x.p.r;
                    }
                    if (w.l.color == Color.Black && w.r.color == Color.Black)
                    {
                        w.color = Color.Red;
                        x = x.p;
                    }
                    else
                    {
                        if (w.r.color == Color.Black)
                        {
                            w.l.color = Color.Black;
                            w.color = Color.Red;
                            RightRotate(w);
                            w = x.p.r;
                        }
                        w.color = x.p.color;
                        x.p.color = Color.Black;
                        w.r.color = Color.Black;
                        LeftRotate(x.p);
                        x = root;
                    }
                }
                else
                {
                    Node w = x.p.l;
                    if (w.color == Color.Red)
                    {
                        w.color = Color.Black;
                        x.p.color = Color.Red;
                        RightRotate(x.p);
                        w = x.p.l;
                    }
                    if (w.r.color == Color.Black && w.l.color == Color.Black)
                    {
                        w.color = Color.Red;
                        x = x.p;
                    }
                    else
                    {
                        if (w.l.color == Color.Black)
                        {
                            w.r.color = Color.Black;
                            w.color = Color.Red;
                            LeftRotate(w);
                            w = x.p.l;
                        }
                        w.color = x.p.color;
                        x.p.color = Color.Black;
                        w.l.color = Color.Black;
                        RightRotate(x.p);
                        x = root;
                    }
                }
            }
            x.color = Color.Black;
        }

        private Node Successor(Node x)
        {
            if (x.r == nil)
            {//Go up untill you are at nil, or untill you are the left child
                while (x.p != nil && x != x.p.l)
                {
                    x = x.p;
                }
                return x.p;
            }
            else
            {//Go right, and then left as much as you can
                x = x.r;
                while (x.l != nil)
                    x = x.l;
                return x;
            }
        }

        private Node Predecessor(Node x)
        {
            if (x.r == nil)
            {//Go up untill you are at nil, or untill you are the right child
                while (x.p != nil && x != x.p.r)
                {
                    x = x.p;
                }
                return x.p;
            }
            else
            {//Go left, and then right as much as you can
                x = x.l;
                while (x.r != nil)
                    x = x.r;
                return x;
            }
        }

        public T Get(T x)
        {
            return Find(x).element;
        }

        public T Get(T x, Compare newComparere)
        {
            return Find(x, newComparere).element;
        }

        private Node Find(T z)
        {
            return Find(z, comparere);
        }

        private Node Find(T z, Compare newComparere)
        {
            Node x = root;
            while (x != nil)
            {
                int cmp = newComparere(z, x.element);
                if (cmp == 0)
                    break;
                x = cmp < 0 ? x.l : x.r;
            }
            return x;
        }

        public RedBlackTree<T> CloneShallow()
        {
            RedBlackTree<T> clone = new RedBlackTree<T>(comparere);
            clone.root = CloneShallow(root, clone.nil);
            //clone.InvariantCheck();
            return clone;
        }

        private Node CloneShallow(Node node, Node newNil)
        {
            if (node == nil)
                return newNil;
            Node left = CloneShallow(node.l, newNil);
            Node right = CloneShallow(node.r, newNil);
            Node n = new Node(left, right, newNil, node.element);
            n.lSize = node.lSize;
            n.rSize = node.rSize;
            left.p = n;
            right.p = n;
            return n;
        }



        public delegate int Match(T item);
        public RedBlackTree<T> GetSubTreeMatching(Match comp)
        {
            Node n = root;
            while (n != nil)
            {
                int c = comp(n.element);
                if (c == 0)
                    break;
                n = c == -1 ? n.l : n.r;
            }
            if (n == nil)
                return new RedBlackTree<T>(comparere);
            RedBlackTree<T> returner = new RedBlackTree<T>(comparere);
            returner.root = CloneShallow(n, returner.nil);
            returner.root.color = Color.Black;
            //returner.InvariantCheck();
            return returner;
        }



        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #region enumerator
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        public Enumerator GetMyEnumerator()
        {
            return new Enumerator(this);
        }

        public Enumerator GetEnumerator(T start)
        {
            return new Enumerator(this, start);
        }

        public Enumerator GetEnumerator(int startIndex)
        {
            return new Enumerator(this, this[startIndex]);
        }

        public Enumerator GetEnumeratorReversed()
        {

            return new Enumerator(this, true);
        }

        public class Enumerator : IEnumerator<T>
        {
            private Node node;
            private RedBlackTree<T> tree;
            private bool start;
            public int Index { get; private set; }

            public Enumerator(RedBlackTree<T> t)
            {
                tree = t;
                Reset();
            }

            public Enumerator(RedBlackTree<T> t, T start)
            {
                tree = t;
                node = tree.Find(start);
                this.start = true;
            }

            public Enumerator(RedBlackTree<T> t, bool reversed)
            {
                if (reversed)
                {

                    tree = t;
                    node = tree.root;
                    while (node.r != tree.nil)
                        node = node.r;
                    start = true;
                    Index = t.Count - 1;
                }
                else
                {
                    tree = t;
                    Reset();
                }
            }

            public void Dispose()
            {
                node = null;
            }

            public bool MoveNext()
            {
                Node n = node;
                if (start)
                {
                    start = false;
                    return node != tree.nil;
                }
                else
                {
                    n = tree.Successor(n);
                    Index++;
                    if (n == tree.nil)
                        return false;
                    node = n;
                }
                return true;
            }

            public bool MovePrevious()
            {
                Node n = node;
                if (start)
                    start = false;
                else
                {
                    n = tree.Predecessor(n);
                    Index--;
                    if (n != tree.nil)
                        node = n;
                }
                return node != tree.nil;
            }

            public void Reset()
            {
                node = tree.root;
                while (node.l != tree.nil)
                    node = node.l;
                start = true;
                Index = 0;
            }

            public T Current
            {
                get { return node.element; }
            }

            object IEnumerator.Current
            {
                get { return node.element; }
            }
        }

        #endregion

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
