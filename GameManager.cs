using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LeapWoF.Interfaces;

namespace LeapWoF
{

    /// <summary>
    /// The GameManager class, handles all game logic
    /// </summary>
    public class GameManager
    {

        /// <summary>
        /// The input provider
        /// </summary>
        private IInputProvider inputProvider;

        /// <summary>
        /// The output provider
        /// </summary>
        private IOutputProvider outputProvider;

        private string TemporaryPuzzle;
        private string HiddenPuzzleDisplay;

        public HashSet<char> charGuessList = new HashSet<char>();

        public GameState GameState { get; private set; }

        public GameManager() : this(new ConsoleInputProvider(), new ConsoleOutputProvider())
        {

        }

        public GameManager(IInputProvider inputProvider, IOutputProvider outputProvider)
        {
            if (inputProvider == null)
                throw new ArgumentNullException(nameof(inputProvider));
            if (outputProvider == null)
                throw new ArgumentNullException(nameof(outputProvider));

            this.inputProvider = inputProvider;
            this.outputProvider = outputProvider;

            GameState = GameState.WaitingToStart;
        }

        /// <summary>
        /// Manage game according to game state
        /// </summary>
        public void StartGame()
        {
            InitGame();

            while (true)
            {

                PerformSingleTurn();

                if (GameState == GameState.RoundOver)
                {
                    StartNewRound();
                    continue;
                }

                if (GameState == GameState.GameOver)
                {
                    outputProvider.WriteLine("Game over");
                    break;
                }
            }
        }

        public string HidePuzzleSolution(string solution)
        {
            StringBuilder puzzle = new StringBuilder();
            for (int i =0; i < solution.Length; i++)
            {
                var toAppend = charGuessList.Contains(solution[i]) ? solution[i] : '_';
                puzzle.Append(toAppend);
                if (i != solution.Length - 1)
                {
                    puzzle.Append(' ');
                }
            }
            
            return puzzle.ToString();
        }
        public void StartNewRound()
        {
            TemporaryPuzzle = "pineapples";
            HiddenPuzzleDisplay = HidePuzzleSolution(TemporaryPuzzle);

            // update the game state
            GameState = GameState.RoundStarted;
        }

        public void PerformSingleTurn()
        {
            //outputProvider.Clear();
            DrawPuzzle();
            outputProvider.WriteLine("Type 1 to spin, 2 to solve");
            GameState = GameState.WaitingForUserInput;

            var action = inputProvider.Read();

            switch (action)
            {
                case "1":
                    Spin();
                    break;
                case "2":
                    Solve();
                    break;
            }

        }

        /// <summary>
        /// Draw the puzzle
        /// </summary>
        private void DrawPuzzle()
        {
            outputProvider.WriteLine("The puzzle is:");
            outputProvider.WriteLine(HiddenPuzzleDisplay);
            outputProvider.WriteLine();
        }

        /// <summary>
        /// Spin the wheel and do the appropriate action
        /// </summary>
        public void Spin()
        {
            outputProvider.WriteLine("Spinning the wheel...");
            //TODO - Implement wheel + possible wheel spin outcomes
            GuessLetter();
        }

        public void Solve()
        {
            outputProvider.Write("Please enter your solution:");
            var guess = inputProvider.Read();
        }
        public void GuessLetter()
        {
            GameState = GameState.GuessingLetter;
            outputProvider.Write("Please guess a letter: ");
            var guess = inputProvider.Read();
            charGuessList.Add(guess[0]); //guess is in string format, this effectively converts to char; TODO, add input validation to reject non-single letter input
            outputProvider.WriteLine("Letters Guessed: " + String.Join(" ", charGuessList));
            HiddenPuzzleDisplay = HidePuzzleSolution(TemporaryPuzzle);
            
        }

        /// <summary>
        /// Optional logic to accept configuration options
        /// </summary>
        public void InitGame()
        {

            outputProvider.WriteLine("Welcome to Wheel of Fortune!");
            StartNewRound();
        }
    }
}
