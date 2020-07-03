using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    
    public class GameLog : LogHeader
    {
        public GameInfo GameInfo { get; set; }
        public string Name { get; set; }
        
    }
}
