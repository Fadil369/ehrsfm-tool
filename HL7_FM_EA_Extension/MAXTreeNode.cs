﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MAX_EA.MAXSchema;

namespace HL7_FM_EA_Extension
{
    public class MAXTreeNodeUtils
    {
        /*
         * Converts a flat MAX model to a tree.
         * Will also fill a id to treeNode Dictionary.
         */
        public static TreeNode ToTree(ModelType model, Dictionary<string, TreeNode> treeNodes)
        {
            // Create TreeNodes lookup map
            foreach (ObjectType maxObj in model.objects)
            {
                treeNodes[maxObj.id] = new TreeNode();
                treeNodes[maxObj.id].metadata = maxObj;
            }

            // Now create tree
            TreeNode root = null;
            foreach (ObjectType maxObj in model.objects)
            {
                // This is a root element when it has no parent
                if (string.IsNullOrEmpty(maxObj.parentId))
                {
                    // We expect only 1 root which is the HL7-FM or HL7-FM-Profile
                    root = treeNodes[maxObj.id];
                }
                else if (treeNodes.ContainsKey(maxObj.parentId))
                {
                    TreeNode node = treeNodes[maxObj.id];
                    TreeNode parent = treeNodes[maxObj.parentId];
                    parent.children.Add(node);
                    node.parent = parent;
                }
                else
                {
                    Console.WriteLine("Warning: parent not found");
                }
            }

            // Add Relationships
            foreach (RelationshipType maxRel in model.relationships)
            {
                treeNodes[maxRel.sourceId].relationships.Add(maxRel);
            }
            return root;
        }
    }

    public class TreeNode
    {
        public TreeNode parent;
        public ObjectType metadata;
        public ObjectType instruction;
        public bool hasInstruction
        {
            get { return instruction != null;  }
        }
        public List<RelationshipType> relationships = new List<RelationshipType>();
        public bool hasConsequenceLinks
        {
            get { return relationships.Any(t => "ConsequenceLink".Equals(t.stereotype)); }
        }
        public IEnumerable<RelationshipType> consequenceLinks
        {
            get { return relationships.Where(t => "ConsequenceLink".Equals(t.stereotype)); }
        }
        public bool include = false;
        public List<TreeNode> children = new List<TreeNode>();

        private void ToObjectList(TreeNode node, List<ObjectType> objects)
        {
            if (node.metadata != null)
            {
                objects.Add(node.metadata);
            }
            foreach (TreeNode child in node.children)
            {
                ToObjectList(child, objects);
            }
        }

        /*
         * Convert a tree back to a list of MAX Objects starting with some node
         * and following its children recursively.
         */
        public List<ObjectType> ToObjectList()
        {
            List<ObjectType> objects = new List<ObjectType>();
            ToObjectList(this, objects);
            return objects;
        }

        private void ToRelationshipList(TreeNode node, List<RelationshipType> relationships)
        {
            relationships.AddRange(node.relationships);
            foreach (TreeNode child in node.children)
            {
                ToRelationshipList(child, relationships);
            }
        }

        /*
         * Convert a tree back to a list of MAX Relationships starting with some node
         * and following its children recursively.
         */
        public List<RelationshipType> ToRelationshipList()
        {
            List<RelationshipType> relationships = new List<RelationshipType>();
            ToRelationshipList(this, relationships);
            return relationships;
        }
    }
}
