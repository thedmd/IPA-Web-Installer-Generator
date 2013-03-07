using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IPATools.Utilities
{
    public class Utils
    {
        public static void CopyStream(Stream from, Stream to)
        {
            CopyStream(from, to, 4096);
        }

        public static void CopyStream(Stream from, Stream to, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int count;
            while ((count = from.Read(buffer, 0, buffer.Length)) != 0)
                to.Write(buffer, 0, count);
        }
    }
}
