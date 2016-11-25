//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co KG.">
// This file contains trade secrets of Avira Operations GmbH & Co KG. No part may be reproduced
// or transmitted in any form by any means or for any purpose without the express written permission
// of Avira Operations GmbH & Co KG.</copyright>
//----------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;

namespace Aos.Messaging
{
    /// <summary>
    /// Stream which writes and reads string messages.
    /// Currently the length of the messages is limited to maximum unsigned integer.
    /// </summary>
    public class MessageStream
    {
        public const string EndOfStream = "E5661997-FE27-4181-93D5-8EE30FED087E";
        private Stream stream;

        public MessageStream(Stream stream)
        {
            this.stream = stream;
        }

        public void WriteMessage(string message)
        {
            int outLen = Encoding.UTF8.GetByteCount(message);
            byte[] outBuffer = new byte[outLen + 2];
            Encoding.UTF8.GetBytes(message, 0, message.Length, outBuffer, 2);

            int len = outLen;
            if (len > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            outBuffer[0] = (byte)(len / 256);
            outBuffer[1] = (byte)(len & 255);
            this.stream.Write(outBuffer, 0, outBuffer.Length);
            this.stream.Flush();
        }

        public string ReadMessage()
        {
            int len = this.stream.ReadByte();
            if (len == -1)
            {
                return EndOfStream;
            }

            len *= 256;
            len += this.stream.ReadByte();

            byte[] inBuffer = new byte[len];
            this.stream.Read(inBuffer, 0, len);
            string message = Encoding.UTF8.GetString(inBuffer);
            return message;
        }
    }
}
