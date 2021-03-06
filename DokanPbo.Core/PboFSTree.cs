﻿using System;
using SwiftPbo;
using DokanNet;
using System.Collections.Generic;
using System.Linq;

namespace DokanPbo
{
    public class PboFSTree
    {
        private ArchiveManager archiveManager;
        private PboFSFolder root;
        private Dictionary<string, PboFSNode> fileTreeLookup;

        public PboFSTree(ArchiveManager archiveManager)
        {
            this.archiveManager = archiveManager;

            createFileTree();
        }

        public IList<FileInformation> FilesForPath(string path)
        {
            PboFSNode node = this.fileTreeLookup[path];
            if (node.GetType() == typeof(PboFSFolder))
            {
                return ((PboFSFolder) node).Children.Values.Select(f => f.FileInformation).ToList();
            }

            return new FileInformation[0];
        }

        public FileInformation FileInfoForPath(string path)
        {
            PboFSNode node = this.fileTreeLookup[path];
            if (this.fileTreeLookup.TryGetValue(path, out node))
            {
                return node.FileInformation;
            }

            return new FileInformation();
        }

        private void createFileTree()
        {
            this.root = new PboFSFolder(null);
            this.fileTreeLookup = new Dictionary<string, PboFSNode>();
            this.fileTreeLookup["\\"] = this.root;

            foreach (string filePath in this.archiveManager.FilePathToArchive.Keys)
            {
                PboArchive archive = this.archiveManager.FilePathToArchive[filePath];
                FileEntry file = this.archiveManager.FilePathToFileEntry[filePath];

                PboFSFolder currentFolder = root;
                var currentPath = "\\";
                var paths = filePath.Split('\\');

                // Create folder for all sub paths
                for (int i = 1; i < paths.Length - 1; i++)
                {
                    var folderName = paths[i];
                    currentPath += folderName;

                    PboFSFolder folder = null;
                    if (!this.fileTreeLookup.ContainsKey(currentPath))
                    {
                        this.fileTreeLookup[currentPath] = new PboFSFolder(folderName);
                    }
                    folder = (PboFSFolder) this.fileTreeLookup[currentPath];

                    if (!currentFolder.Children.ContainsKey(folderName))
                    {
                        currentFolder.Children[folderName] = folder;
                    }

                    currentFolder = (PboFSFolder) currentFolder.Children[folderName];
                    currentPath += "\\";
                }

                var fileName = paths[paths.Length - 1];
                var fileNode = new PboFSFile(fileName, archive, file);
                currentFolder.Children[fileName] = fileNode;
                this.fileTreeLookup[filePath] = fileNode;
            }
        }
    }
}
