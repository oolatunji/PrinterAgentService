using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestPAS
{
    class Program
    {
        static void Main(string[] args)
        {
            PASLibrary.PASWorkflow.SendRequest();
            Console.ReadLine();
        }
    }
}
