using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonism
{
    public abstract class GameObject
    {
        public GameObject(LAYER layer) 
        {
            this.layer = layer;
        }
        public LAYER layer { get; private set; }
        public void SetLayer(LAYER objectType) => this.layer = objectType;

        public string name { get; protected set; } = "";

        public abstract void Update();

        public bool isDestroyOnLoad { get; private set; } = false;
        public void DestroyOnLoad() => isDestroyOnLoad = true;  

    }
}
