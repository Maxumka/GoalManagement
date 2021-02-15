using System;
using System.Collections.Generic;
using System.Linq;

namespace GoalManagement.BLogic.Extensions
{
    public static class TreeExtensions
    {
        public static Tree<T> ToTree<T>(this IList<T> items, Func<T, T, bool> parentSelector)
        {
            var lookup = items.ToLookup(item => items.FirstOrDefault(parent => parentSelector(parent, item)), child => child);

            return Tree<T>.FromLookup(lookup);
        }

        public static IEnumerable<Tree<T>> ToTreeEnumerable<T>(this Tree<T> tree)
        {
            return tree.Children
                       .Flatten(n => n.Children);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childrenSelector)
        {
            return items.SelectMany(c => childrenSelector(c).Flatten(childrenSelector))
                        .Concat(items);
        }

        public static IEnumerable<T> GetAllDescendants<T>(this Tree<T> parentTree)
        {
            return parentTree.Children
                             .Flatten(n => n.Children)
                             .Select(n => n.Data)
                             .ToList();
        }
    }
}
