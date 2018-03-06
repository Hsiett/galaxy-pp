using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Microsoft.Xna.Framework.Graphics;

//using Aga.Controls.Tree.NodeControls;

namespace Galaxy_Editor_2.Dialog_Creator.Complex_properties
{
    public partial class TextureBrowserDialog : Form
    {
        
        
        private  Dictionary<string, Node> folderNodes = new Dictionary<string, Node>();
        private  Dictionary<Node, Node> parents = new Dictionary<Node, Node>();
        
        public string SelectedPath { get; private set; }
        public Texture2D SelectedTexture { get; private set; }
        private TreeNodeAdv initialSelection;

        private static Image FolderOpen = Properties.Resources.FolderOpen;
        private static Image FolderClosed = Properties.Resources.FolderClosed;
        private static Image File = Properties.Resources.File;
        private  List<string> paths=null;
        private  TreeModel model = new TreeModel();
        private  List<Node> textureNodes = new List<Node>();

        public TextureBrowserDialog()
        {
            InitializeComponent();
            
            //int initialSelectedNodeIndex = -1;
            graphicsControl.Size = new Size(0, 0);
            ResizePanel();

            if (paths == null)
            {
                paths = TextureLoader.GetAllPaths();
                foreach (string path in paths)
                {
                    //Check if the folderNode is created
                    Node parent = MakeFolderNode(path);
                    Node textureNode = new Node(path.Substring(path.LastIndexOfAny(new[] { '\\', '/' }) + 1));
                    textureNode.Image = File;
                    textureNode.Tag = textureNodes.Count;

                    //if (path == initialSelectedPath)
                    //    initialSelectedNodeIndex = textureNodes.Count;
                    textureNodes.Add(textureNode);
                    if (parent != null)
                        parents[textureNode] = parent;
                }

                //Build the tree view
                includeSnapshots.Add(new List<int>());
                int i = 0;
                foreach (Node textureNode in textureNodes)
                {
                    includeSnapshots[0].Add(i++);
                    Node node = textureNode;
                    Node parent = parents.ContainsKey(node) ? parents[node] : null;
                    while (parent != null)
                    {
                        if (parent.Nodes.Contains(node))
                        {
                            node = null;
                            break;
                        }
                        parent.Nodes.Add(node);
                        node = parent;
                        parent = parents.ContainsKey(node) ? parents[node] : null;
                    }
                    if (node != null && !model.Nodes.Contains(node))
                        model.Nodes.Add(node);
                }
                TVTextures.Model = model;
            }
        }

        public void setupNewSearch(string initialSelectedPath)
        {
           
            for(int i = 0; i < paths.Count; ++i)
            {
                if (paths[i] == initialSelectedPath)
                {
                    initialSelection = TVTextures.FindNode(model.GetPath(textureNodes[i]));
                    break;
                }
            }
        }

        private Node MakeFolderNode(string path)
        {
            if (!path.Contains("\\") && !path.Contains("/"))
                return null;
            string folderPath = path.Remove(path.LastIndexOfAny(new []{'\\', '/'}));
            if (folderNodes.ContainsKey(folderPath))
                return folderNodes[folderPath];
            Node parentFolder = MakeFolderNode(folderPath);
            Node folder = new Node(folderPath.Substring(folderPath.LastIndexOfAny(new[] { '\\', '/' }) + 1));
            folder.Image = FolderClosed;
            if (parentFolder != null)
                parents[folder] = parentFolder;
            folderNodes[folderPath] = folder;
            return folder;
        }

        private string prevSearchText = "";
        List<List<int>> includeSnapshots = new List<List<int>>();

        private void TBSearch_TextChanged(object sender, EventArgs e)
        {
            string newSearchText = TBSearch.Text.ToLower();
            if (prevSearchText == newSearchText)
                return;
            TVTextures.BeginUpdate();
            if (prevSearchText == "" || newSearchText.StartsWith(prevSearchText))
            {
                //Stuff added. Find latest snapshot
                List<int> snapshot = includeSnapshots[prevSearchText.Length];
                //Remove stuff from it
                List<int> nextSnapshot = new List<int>();
                List<Node> possibleDeleteFolders = new List<Node>();
                foreach (int s in snapshot)
                {
                    if (paths[s].ToLower().Contains(newSearchText))
                        nextSnapshot.Add(s);
                    else
                    {
                        //Remove it from the tree view
                        Node node = textureNodes[s];
                        if (parents.ContainsKey(node))
                        {
                            Node parent = parents[node];
                            parent.Nodes.Remove(node);
                            if (!possibleDeleteFolders.Contains(parent))
                                possibleDeleteFolders.Add(parent);
                        }
                        else
                            model.Nodes.Remove(node);
                    }
                }
                //Remove folders with no children
                while (possibleDeleteFolders.Count > 0)
                {
                    Node node = possibleDeleteFolders[0];
                    possibleDeleteFolders.RemoveAt(0);
                    if (node.Nodes.Count == 0)
                    {
                        if (parents.ContainsKey(node))
                        {
                            Node parent = parents[node];
                            parent.Nodes.Remove(node);
                            if (!possibleDeleteFolders.Contains(parent))
                                possibleDeleteFolders.Add(parent);
                        }
                        else
                            model.Nodes.Remove(node);
                    }
                }
                //Update includeSnapshots
                while (includeSnapshots.Count <= newSearchText.Length)
                    includeSnapshots.Add(null);
                includeSnapshots[newSearchText.Length]=nextSnapshot;
            }
            else if (newSearchText == "" || prevSearchText.StartsWith(newSearchText))
            {
                //We deleted something in the back.
                //Check if we already have the target snapshot cached
                List<int> snapshot = includeSnapshots[newSearchText.Length];
                
                if (snapshot != null)
                {
                    //Add stuff to match current snapshot
                    var newSnapshotEnum = snapshot.GetEnumerator();
                    var currSnapshotEnum = includeSnapshots[prevSearchText.Length].GetEnumerator();
                    bool currHasMore = currSnapshotEnum.MoveNext();
                    while (newSnapshotEnum.MoveNext())
                    {
                        if (currHasMore && currSnapshotEnum.Current == newSnapshotEnum.Current)
                        {
                            currHasMore = currSnapshotEnum.MoveNext();
                            continue;
                        }
                        Node node = textureNodes[newSnapshotEnum.Current];
                        if (parents.ContainsKey(node))
                        {
                            Node n = node;
                            Node parent = parents[n];
                            parent.Nodes.Add(n);
                            while (parent.Nodes.Count == 1)
                            {
                                n = parent;
                                if (parents.ContainsKey(n))
                                {
                                    parent = parents[n];
                                    parent.Nodes.Add(n);
                                }
                                else
                                {
                                    model.Nodes.Add(n);
                                    break;
                                }
                            }
                        }
                        else
                            model.Nodes.Add(node);
                    }
                    //Update snapshots
                    while (includeSnapshots.Count > newSearchText.Length + 1)
                        includeSnapshots.RemoveAt(includeSnapshots.Count - 1);
                }
                else
                {
                    //Loop through and add stuff that matches

                    List<int> nextSnapshot = new List<int>();
                    var currSnapshotEnum = includeSnapshots[prevSearchText.Length].GetEnumerator();
                    bool currHasMore = currSnapshotEnum.MoveNext();
                    for (int i = 0; i < textureNodes.Count; i++)
                    {
                        if (currHasMore && currSnapshotEnum.Current == i)
                        {
                            currHasMore = currSnapshotEnum.MoveNext();
                            nextSnapshot.Add(i);
                            continue;
                        }
                        if (!paths[i].Contains(newSearchText))
                            continue;
                        nextSnapshot.Add(i);
                        Node node = textureNodes[i];
                        if (parents.ContainsKey(node))
                        {
                            Node n = node;
                            Node parent = parents[n];
                            parent.Nodes.Add(n);
                            while (parent.Nodes.Count == 1)
                            {
                                n = parent;
                                if (parents.ContainsKey(n))
                                {
                                    parent = parents[n];
                                    parent.Nodes.Add(n);
                                }
                                else
                                {
                                    model.Nodes.Add(node);
                                    break;
                                }
                            }
                        }
                        else
                            model.Nodes.Add(node);
                    }
                    //Update snapshots
                    while (includeSnapshots.Count > newSearchText.Length + 1)
                        includeSnapshots.RemoveAt(includeSnapshots.Count - 1);
                    includeSnapshots[newSearchText.Length] = nextSnapshot;
                }
            }
            else
            {
                //None of them are prefixes of the other.
                //Find nearest valid snapshot
                int validSnapshotIndex;
                for (validSnapshotIndex = 0; validSnapshotIndex < Math.Min(prevSearchText.Length, newSearchText.Length); validSnapshotIndex++)
                {
                    if (prevSearchText[validSnapshotIndex] != newSearchText[validSnapshotIndex])
                        break;
                }
                List<int> newestValidSnapshot = includeSnapshots[prevSearchText.Length];
                while (includeSnapshots.Count > validSnapshotIndex + 1)
                    includeSnapshots.RemoveAt(includeSnapshots.Count - 1);

                List<int> nextSnapshot = new List<int>();
                var newestValidEnum = newestValidSnapshot.GetEnumerator();
                bool newestEnumHasMore = newestValidEnum.MoveNext();
                List<Node> possibleDeleteFolders = new List<Node>();
                for (int i = 0; i < textureNodes.Count; i++)
                {
                    bool shouldBeAdded = paths[i].Contains(newSearchText);
                    if (shouldBeAdded)
                        nextSnapshot.Add(i);
                    Node node = textureNodes[i];
                    if (newestEnumHasMore && newestValidEnum.Current == i)
                    {
                        if (!shouldBeAdded)
                        {
                            //Remove it from the tree view
                            if (parents.ContainsKey(node))
                            {
                                Node parent = parents[node];
                                parent.Nodes.Remove(node);
                                if (!possibleDeleteFolders.Contains(parent))
                                    possibleDeleteFolders.Add(parent);
                            }
                            else
                                model.Nodes.Remove(node);
                        }
                        newestEnumHasMore = newestValidEnum.MoveNext();
                        continue;
                    }
                    if (!shouldBeAdded)
                        continue;
                    if (parents.ContainsKey(node))
                    {
                        Node n = node;
                        Node parent = parents[n];
                        parent.Nodes.Add(n);
                        while (parent.Parent == null)
                        {
                            n = parent;
                            if (parents.ContainsKey(n))
                            {
                                parent = parents[n];
                                parent.Nodes.Add(n);
                            }
                            else
                            {
                                model.Nodes.Add(n);
                                break;
                            }
                        }
                    }
                    else
                        model.Nodes.Add(node);
                }
                //Remove folders with no children
                while (possibleDeleteFolders.Count > 0)
                {
                    Node node = possibleDeleteFolders[0];
                    possibleDeleteFolders.RemoveAt(0);
                    if (node.Nodes.Count == 0)
                    {
                        if (parents.ContainsKey(node))
                        {
                            Node parent = parents[node];
                            parent.Nodes.Remove(node);
                            if (!possibleDeleteFolders.Contains(parent))
                                possibleDeleteFolders.Add(parent);
                        }
                        else
                            model.Nodes.Remove(node);
                    }
                }
                //Update includeSnapshots
                while (includeSnapshots.Count <= newSearchText.Length)
                    includeSnapshots.Add(null);
                includeSnapshots[newSearchText.Length] = nextSnapshot;
            }
            TVTextures.EndUpdate();
            prevSearchText = newSearchText;
        }

        private void TVTextures_Expanded(object sender, TreeViewAdvEventArgs e)
        {
            if (e.Node.Tag != null)
                ((Node) e.Node.Tag).Image = FolderOpen;
        }

        private void TVTextures_Collapsed(object sender, TreeViewAdvEventArgs e)
        {
            if (e.Node.Tag != null)
                ((Node)e.Node.Tag).Image = FolderClosed;
        }

        private void TVTextures_SelectionChanged(object sender, EventArgs e)
        {
            if (TVTextures.SelectedNode == null)
                return;
            Node node = (Node) TVTextures.SelectedNode.Tag;
            if (node.Tag == null)
                return;
            int index = (int) node.Tag;
            if (SelectedPath != null)
            {
                TextureLoader.Unload(SelectedPath);
            }
            SelectedPath = paths[index];
            Texture2D texture = TextureLoader.Load(SelectedPath, graphicsControl.GraphicsDevice);
            SelectedTexture = texture;
            BTNOK.Enabled = texture != null;
            graphicsControl.SetBackgroundImage(texture);
            if (texture == null)
                SelectedPath = null;
            Size size = texture == null ? new Size(0, 0) : new Size(texture.Width, texture.Height);
            graphicsControl.Size = size;
            graphicsControl.Invalidate();
            ResizePanel();
        }

        private void ResizePanel()
        {
            Point panelPos = new Point();
            Size desiredSize = graphicsControl.Size;
            Size panelSize = new Size(desiredSize.Width + 2, desiredSize.Height + 2);
            Size parentSize = splitter.Panel2.Size;

            if (panelSize.Width > parentSize.Width)
                panelSize.Width = parentSize.Width;
            if (panelSize.Height > parentSize.Height)
                panelSize.Height = parentSize.Height;

            panelPos.X = (parentSize.Width - panelSize.Width) / 2;
            panelPos.Y = (parentSize.Height - panelSize.Height) / 2;

            graphicsControlPanel.Location = panelPos;
            graphicsControlPanel.Size = panelSize;
        }

        private void splitter_Panel2_SizeChanged(object sender, EventArgs e)
        {
            ResizePanel();
        }

        private void TextureBrowserDialog_Load(object sender, EventArgs e)
        {
            if (initialSelection != null)
            {
                TVTextures.SelectedNode = initialSelection;
                TVTextures.EnsureVisible(initialSelection);
                TVTextures.ScrollTo(initialSelection);
                TVTextures.Focus();
            }
        }
    }
}
