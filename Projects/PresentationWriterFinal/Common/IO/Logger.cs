using System;
using System.IO;

namespace HSR.PresWriter.Common.IO
{
    public static class Logger
    {
        public static void Log(Exception e)
        {
            var fn = @"Exceptions\" + DateTime.Now.Date.ToString("dd-MM-yy") + ".txt";
            if (!Directory.Exists(@"Exceptions"))
                Directory.CreateDirectory(@"Exceptions");
            if (!File.Exists(fn))
                File.Create(fn);
            using (var fs = new StreamWriter(new FileStream(fn, FileMode.Append, FileAccess.Write)))
            {
                fs.WriteLine(e.ToString());
                fs.Flush();
            }
        }
    }
}
