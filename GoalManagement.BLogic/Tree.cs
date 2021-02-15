using System;
using System.Collections.Generic;
using System.Linq;

namespace GoalManagement.BLogic
{
    public class Tree<T>
    {
        public T Data { get; }

        public Tree<T> Parent { get; private set; }

        public ICollection<Tree<T>> Children { get; }

        private Tree(T data)
        {
            Children = new LinkedList<Tree<T>>();
            Data = data;
        }

        public static Tree<T> FromLookup(ILookup<T, T> lookup)
        {
            var rootData = lookup.Count == 1 ? lookup.First().Key : default;
            var root = new Tree<T>(rootData);
            root.LoadChildren(lookup);
            return root;
        }

        private void LoadChildren(ILookup<T, T> lookup)
        {
            foreach (var data in lookup[Data])
            {
                var child = new Tree<T>(data) { Parent = this };
                Children.Add(child);
                child.LoadChildren(lookup);
            }
        }
    }
}
