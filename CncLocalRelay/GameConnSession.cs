using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CncLocalRelay
{
    public class GameConnSession
    {
        public int sessionId;
        public int connectionId;
        public int localport;

        public bool EqualsConSess(GameConnSession compare)
        {
            if (compare == null)
            {
                return false;
            }

            return sessionId == compare.sessionId
                && connectionId == compare.connectionId;
        }
    }
}
