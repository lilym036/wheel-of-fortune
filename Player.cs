using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LeapWoF
{
    internal class Player
    {
        public string playerName { get; private set; }
        public int totalScore { get; private set; }

        public Player(string playerName)
        {
            this.playerName = playerName;
            this.totalScore = 0;
        }

        public void UpdateTotalScore(int roundScore)
        {
            this.totalScore += roundScore;
        }
    }
}

