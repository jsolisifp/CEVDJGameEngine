using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class GameManager
    {

        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameManager();
                return instance;
            }
        }

        private int score = 0;
        private List<Pin> pins;

        public void InitializePins(List<Pin> pins)
        {
            this.pins = pins;
        }

        public void AddScore(int points)
        {
            score += points;
            Console.WriteLine("Score: " + score);
        }

        public void ResetGame()
        {
            score = 0;
            foreach (var pin in pins)
            {
                pin.Reset();
            }
        }
    }
}
