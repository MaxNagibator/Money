using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceWorker.Api
{
    public class ApiHelper
    {
        public static Type GetType(string tt)
        {
            Type t = Type.GetType("ServiceWorker." + tt);
            return t;
        }
    }
}
