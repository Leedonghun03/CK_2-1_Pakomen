using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;


public class Singleton<T> where T : class, new()
{
    private static T _instance;
    private static readonly object _lockObject = new object();

    public static T Instance
    {
        get
        {
            if (null == _instance)
            {
                lock (_lockObject)
                {
                    if (null == _instance)
                    {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
}

