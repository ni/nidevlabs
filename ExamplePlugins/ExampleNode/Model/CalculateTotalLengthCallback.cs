using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamplePlugins.ExampleNode.Model
{
    /// <summary>
    /// An example class that is called into by <see cref="CalculateTotalLengthDfirNode"/> 
    /// </summary>
    public static class CalculateTotalLengthCallback
    {
        /// <summary>
        /// An example method that is called by <see cref="CalculateTotalLengthDfirNode"/> 
        /// </summary>
        /// <param name="names">An array of names</param>
        /// <param name="extraName">An extra name</param>
        /// <returns>The total length of all the names passed in.</returns>
        public static int CalculateTotalLength(string[] names, string extraName)
        {
            int length = 0;
            foreach (string name in names)
            {
                length += name.Length;
            }
            length += extraName.Length;
            return length;
        }
    }
}
