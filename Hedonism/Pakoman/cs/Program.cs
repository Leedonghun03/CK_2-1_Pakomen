using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonism
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameManager.Instance.Initialize();
            GameManager.Instance.GameProcess();

            //25.06.22.
            //Real Final Version Release.

        }
    }
}