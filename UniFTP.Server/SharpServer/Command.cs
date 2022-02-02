using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpServer
{
    /// <summary>
    /// Order
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Operation instructions
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// parameter
        /// <para>Note: Space will be used as a delimiter, please use it with caution</para>
        /// </summary>
        public List<string> Arguments { get; set; }
        public string Raw { get; set; }
        /// <summary>
        /// raw parameters
        /// </summary>
        public string RawArguments { get; set; }
    }
}
