using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapWoF
{
    internal class SpinWheel
    {
      
        private List<object> segments;
        private Random random;

        public SpinWheel()
        {
            segments = new List<object>
    {
        100, 200, 300, 400, 500, 600, 700, 800, 900, 1000,
        "Bankrupt", "Lose a Turn"
    };
            random = new Random();
        }

        public object Spin()
        {
            int index = random.Next(segments.Count);
            return segments[index];
        }
    }
}
