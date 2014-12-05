using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniFTP.Server.Virtual
{
    [Serializable]
    public class FilePermission
    {
        private char[] _attributes = "---------".ToCharArray();
        
        //public string UserPermission {
        //    get { return _attributes.Substring(0, 3); }
        //}

        //public string GroupPermission
        //{
        //    get { return _attributes.Substring(3, 3); }
        //}

        //public string OtherPermission
        //{
        //    get { return _attributes.Substring(6, 3); }
        //}

        /// <summary>
        /// 可读
        /// </summary>
        public bool CanRead {
            get { return _attributes[0] == 'r'; }
            set
            {
                if (value)
                {
                    _attributes[0] = 'r';
                }
                else
                {
                    _attributes[1] = '-';
                }
            }
        }

        /// <summary>
        /// 可写
        /// </summary>
        public bool CanWrite {
            get { return _attributes[1] == 'w'; }
            set {
                if (value)
                {
                    _attributes[1] = 'w';
                }
                else
                {
                    _attributes[1] = '-';
                }
            }
        }

        /// <summary>
        /// 文件权限
        /// </summary>
        /// <param name="attribute">9位文件标签</param>
        /// <example>rwxrwxrwx</example>
        public FilePermission(string attribute)
        {
            ConvertToAttributes(attribute);
            CheckAttributeString();
        }

        /// <summary>
        /// 文件权限
        /// <para>创建一个默认的只读权限(r-xr-xr-x)</para>
        /// </summary>
        public FilePermission()
        {
            _attributes = "r-xr-xr-x".ToCharArray();
        }

        /// <summary>
        /// 文件权限
        /// </summary>
        /// <param name="attribute">3位权限数字</param>
        /// <example>777</example>
        public FilePermission(int attribute)
        {
            if (!ConvertToAttributes(attribute))
            {
                throw new FormatException("Bad Attribute Format.");
            }
        }

        public override string ToString()
        {
            return new string(_attributes); //FIXED:字符数组转字符串，请务必用这种方式
        }

        private void CheckAttributeString()
        {
            if (new string(_attributes).Replace('r', ' ').Replace('w', ' ').Replace('x', ' ').Replace('-', ' ').Trim() != "")
            {
                //_attributes = "---------";
                throw new FormatException("Bad Attribute Format.");
            }
        }

        /// <summary>
        /// 转换为标准文件权限标签
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private bool ConvertToAttributes(int attribute)
        {
            if (attribute > 777 || attribute < 0 || attribute /100 > 7 || attribute / 10 > 77)
            {
                return false;
            }
            StringBuilder sb = new StringBuilder();
            string attr = attribute.ToString("D3");
            
            for (int i = 0; i < 3; i++)
            {
                switch (attr[i])
                {
                    case '8':
                    case '0':
                        sb.Append("---");
                        break;
                    case '9':
                    case '1':
                        sb.Append("--x");
                        break;
                    case '2':
                        sb.Append("-w-");
                        break;
                    case '3':
                        sb.Append("-wx");
                        break;
                    case '4':
                        sb.Append("r--");
                        break;
                    case '5':
                        sb.Append("r-x");
                        break;
                    case '6':
                        sb.Append("rw-");
                        break;
                    case '7':
                        sb.Append("rwx");
                        break;
                    default:
                        sb.Append("---");
                        break;

                }
            }
            _attributes = sb.ToString(0, 9).ToCharArray();
            return true;
        }

        private void ConvertToAttributes(string attribute)
        {
            if (attribute.Length < 9)
            {
                _attributes = attribute.PadRight(9, '-').ToCharArray();
            }
            else
            {
                _attributes = attribute.Length == 9 ? (attribute).ToCharArray() : attribute.Substring(0, 9).ToCharArray();
            }
        }
    }
}
