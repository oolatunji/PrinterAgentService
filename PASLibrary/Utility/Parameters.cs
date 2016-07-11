using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace PASLibrary
{
    public class Parameters
    {
        static private IDictionary _dic = null;


        static public string Get(string key)
        {
            if (_dic == null)
            {
                _dic = DictionaryFactory.Load("Parameters.xml", "Parameters");
            }

            return (string)_dic[key];
        }
    }
}
