using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpServer
{
    /// <summary>
    /// 命令
    /// </summary>
    public class Command
    {
        /// <summary>
        /// 代号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public List<string> Arguments { get; set; }
        public string Raw { get; set; }
        /// <summary>
        /// 原始参数
        /// </summary>
        public string RawArguments { get; set; }
    }
}
