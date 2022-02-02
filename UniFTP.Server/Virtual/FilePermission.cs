﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniFTP.Server.Virtual
{
    [Serializable]
    public class FilePermission
    {
        private char[] _attributes = "---------".ToCharArray();

        #region Permission bits

        //The convention that the first w is writable (uploaded) and the second w is modifiable

        ///<summary>
        ///Readable
        ///</summary>
        public bool CanRead
        {
            get { return _attributes[0] == 'r'; }
            set
            {
                if (value)
                {
                    _attributes[0] = 'r';
                }
                else
                {
                    _attributes[0] = '-';
                }
            }
        }

        ///<summary>
        ///Writable
        ///</summary>
        public bool CanWrite
        {
            get { return _attributes[1] == 'w'; }
            set
            {
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

        ///<summary>
        ///The group is readable
        ///</summary>
        public bool GroupCanRead
        {
            get { return _attributes[3] == 'r'; }
            set
            {
                if (value)
                {
                    _attributes[3] = 'r';
                }
                else
                {
                    _attributes[3] = '-';
                }
            }
        }

        ///<summary>
        ///Groups are writable
        ///</summary>
        public bool GroupCanWrite
        {
            get { return _attributes[4] == 'w'; }
            set
            {
                if (value)
                {
                    _attributes[4] = 'w';
                }
                else
                {
                    _attributes[4] = '-';
                }
            }
        }

        ///<summary>
        ///Other readable
        ///</summary>
        public bool OtherCanRead
        {
            get { return _attributes[6] == 'r'; }
            set
            {
                if (value)
                {
                    _attributes[6] = 'r';
                }
                else
                {
                    _attributes[6] = '-';
                }
            }
        }

        ///<summary>
        ///Others are writable
        ///</summary>
        public bool OtherCanWrite
        {
            get { return _attributes[7] == 'w'; }
            set
            {
                if (value)
                {
                    _attributes[7] = 'w';
                }
                else
                {
                    _attributes[7] = '-';
                }
            }
        }

        #endregion
        ///<summary>
        ///File permissions
        ///</summary>
        ///<param name="attribute" > a 9-digit file tag</param>
        ///<example>rwxrwxrwx</example>
        public FilePermission(string attribute)
        {
            ConvertToAttributes(attribute);
            CheckAttributeString();
        }

        ///<summary>
        ///File permissions
        ///<para>Create a default read-only permission (r-xr-xr-x</para>).
        ///</summary>
        public FilePermission()
        {
            _attributes = "r-xr-xr-x".ToCharArray();
        }

        ///<summary>
        ///File permissions
        ///</summary>
        ///<param name="attribute" > 3-digit permission number</param>
        ///<example>777</example>
        public FilePermission(int attribute)
        {
            if (!ConvertToAttributes(attribute))
            {
                throw new FormatException("Bad Attribute Format.");
            }
        }

        public override string ToString()
        {
            return new string(_attributes); //Fixed: Character arrays to strings, be sure to use this way
        }

        private void CheckAttributeString()
        {
            if (new string(_attributes).Replace('r', ' ').Replace('w', ' ').Replace('x', ' ').Replace('-', ' ').Trim() != "")
            {
                //_attributes = "---------";
                throw new FormatException("Bad Attribute Format.");
            }
        }

        ///<summary>
        ///Converts to a standard file permissions label
        ///</summary>
        ///<param name="attribute"></param>
        ///<returns></returns>
        private bool ConvertToAttributes(int attribute)
        {
            if (attribute > 777 || attribute < 0 || attribute / 100 > 7 || attribute / 10 > 77)
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
