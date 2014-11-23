using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;

namespace SharpServer
{
    //public enum ResponseCode
    //{
    //    OK = 200,
    //    UTF8_ENCODING_ON = 200,
    //    UNABLE_TO_OPEN_DATA_CONNECTION = 500,
    //    PARAMETER_NOT_RECOGNIZED = 501,
    //    NOT_IMPLEMENTED = 502,
    //    NOT_IMPLEMENTED_FOR_PARAMETER = 504,

    //    FEATURES = 211,
    //    SYSTEM = 215,
        
    //    SERVICE_READY = 220,
    //    QUIT = 221,
    //    TRANSFER_SUCCESSFUL = 226,
    //    ENTERING_PASSIVE_MODE = 227,
    //    ENTERING_EXTENDED_PASSIVE_MODE = 229,
    //    TRANSFER_ABORTED = 426,

    //    LOGGED_IN = 230,
    //    ENABLING_TLS = 234,
    //    USER_OK = 331,
    //    NEED_TWO_FACTOR_CODE = 332,
    //    NOT_LOGGED_IN = 530,

    //    OPENING_DATA_TRANSFER = 150,
    //    FILE_ACTION_COMPLETE = 250,
    //    RENAME_FROM = 350,
    //    FILE_ACTION_NOT_TAKEN = 450,
    //    FILE_NOT_FOUND = 550,
    //    DIRECTORY_NOT_FOUND = 550,
    //    DIRECTORY_EXISTS = 550,
    //    CURRENT_DIRECTORY = 257

    //}

    /// <summary>
    /// 应答
    /// </summary>
    public class Response
    {
        public Response()
        {
            Data = new List<object>();
        }
        /// <summary>
        /// 代号
        /// </summary>
        public string Code { get; set; }
        public string Text { get; set; }
        public bool ShouldQuit { get; set; }
        public List<object> Data { get; set; }
        public CultureInfo Culture { get; set; }

        public ResourceManager ResourceManager { get; set; }

        public Response SetData(params object[] data)
        {
            Data.Clear();
            Data.AddRange(data);

            return this;
        }

        public Response SetCulture(CultureInfo culture)
        {
            this.Culture = culture;

            return this;
        }

        public override string ToString()
        {
            if (this.Culture == null)
            {
                this.Culture = CultureInfo.CurrentCulture;
            }

            if (ResourceManager != null)
            {
                return string.Concat(Code, " ", string.Format(ResourceManager.GetString(Text, Culture), Data.ToArray()));
            }

            if (Text != null)
                return string.Concat(Code, " ", string.Format(Text, Data.ToArray()));
            else
                return Code;
        }
    }
}
