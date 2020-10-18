using System;
using System.Collections.Generic;

namespace AmidUs.Utils
{
    static class RandomExtensions
    {
        // from https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
        public static void Shuffle<T> (this Random rng, List<T> array)
        {
            int n = array.Count;
            while (n > 1) 
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}