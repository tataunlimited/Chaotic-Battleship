using System.Collections.Generic;
using UnityEngine;

// Required for Random.Range

namespace Utility
{
    public static class ListExtensions
    {
        /// <summary>
        /// Shuffles the elements of a list using the Fisher-Yates algorithm.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to be shuffled.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                // Pick a random index from 0 to i
                int randomIndex = Random.Range(0, i + 1);

                // Swap the current element with the element at the random index
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
}