using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

namespace pdfParserByMH
{
    class MainClass
    {

        [STAThread] //For some reason, this is very important for Windows.Forms to work properly!
        static int Main()
        {
            //System.Console.WriteLine("Hello from Main");

            FrontEndProgram program = new FrontEndProgram();
            program.run();
            
            return 0;
        }
    }
}
