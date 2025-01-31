using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapWoF
{
    // Reflects players' scores for a given round.
    internal class Scoreboard
    {
        private Dictionary<string, int> scores;

        public Scoreboard()
        {
            scores = new Dictionary<string, int>();
        }

        public void AddPlayer(string playerName)
        {
            if (!scores.ContainsKey(playerName))
            {
                scores[playerName] = 0;
            }
        }

        public void UpdateScore(string playerName, int points)
        {
            if (scores.ContainsKey(playerName))
            {
                scores[playerName] += points;
            }
        }
        
        //Resets all players' Round score to 0 at start of new round.
        public void ResetScore(string playerName)
        {
            if (scores.ContainsKey(playerName))
            {
                scores[playerName] = 0;
            }
        }

        public int GetScore(string playerName)
        {
            return scores.ContainsKey(playerName) ? scores[playerName] : 0;
        }

        public void DisplayScores()
        {
            foreach (var player in scores)
            {
                Console.WriteLine($"{player.Key}: {player.Value}");
            }
        }
    }
}