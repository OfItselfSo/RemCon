using System;

namespace RemConCommon
{
    /// +------------------------------------------------------------------------------------------------------------------------------+
    /// ¦                                                   TERMS OF USE: MIT License                                                  ¦
    /// +------------------------------------------------------------------------------------------------------------------------------¦
    /// ¦Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    ¦
    /// ¦files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    ¦
    /// ¦modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software¦
    /// ¦is furnished to do so, subject to the following conditions:                                                                   ¦
    /// ¦                                                                                                                              ¦
    /// ¦The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.¦
    /// ¦                                                                                                                              ¦
    /// ¦THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          ¦
    /// ¦WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         ¦
    /// ¦COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   ¦
    /// ¦ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         ¦
    /// +------------------------------------------------------------------------------------------------------------------------------+

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to contain the data sent between the server and client. Note
    /// that the [SerializableAttribute] decoration must be present and any 
    /// user written classes contained within this class must also implement it.
    /// </summary>
    /// <history>
    ///    10 Nov 18  Cynic - Started
    /// </history>
    [SerializableAttribute]
    public class ServerClientData
    {
        // NOTE: this enum also uses the [SerializableAttribute]
        private ServerClientDataContentEnum dataContent = ServerClientDataContentEnum.NO_DATA;
        private string dataStr = "";
        private int dataInt;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        public ServerClientData()
        { }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor. Assumes ServerClientDataContentEnum.USER_DATA
        /// </summary>
        /// <param name="dataIntIn">the integer data value</param>
        /// <param name="dataStrIn">the string data value</param>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        public ServerClientData(int dataIntIn, string dataStrIn)
        {
            DataContent = ServerClientDataContentEnum.USER_DATA;
            DataInt = dataIntIn;
            DataStr = dataStrIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataContentIn">the data content type</param>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        public ServerClientData(ServerClientDataContentEnum dataContentIn)
        {
            DataContent = dataContentIn;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the data content flag
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        public ServerClientDataContentEnum DataContent
        {
            get
            {
                return dataContent;
            }
            set
            {
                dataContent = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the DataStr data value. Will never return null will return empty
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        public string DataStr
        {
            get
            {
                if (dataStr == null) dataStr = "";
                return dataStr;
            }
            set
            {
                dataStr = value;
                if (dataStr == null) dataStr = "";
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the DataInt data value. 
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        public int DataInt
        {
            get
            {
                return dataInt;
            }
            set
            {
                dataInt = value;
            }
        }
    }
}
