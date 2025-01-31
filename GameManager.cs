using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using LeapWoF.Interfaces;
using static System.Collections.Specialized.BitVector32;

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
        private Player currentPlayer;
        private Scoreboard Scoreboard = new Scoreboard();
        private SpinWheel SpinWheel = new SpinWheel();

        public HashSet<char> charGuessSet = new HashSet<char>();
        private List<Player> players = new List<Player>();

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

                PerformTurns();

                if (GameState == GameState.RoundOver)
                {
                    charGuessSet.Clear();
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
                var toAppend = charGuessSet.Contains(solution[i]) ? solution[i] : '_';
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
            charGuessSet.Clear();
            TemporaryPuzzle = "pineapples";
            HiddenPuzzleDisplay = HidePuzzleSolution(TemporaryPuzzle);

            // update the game state
            GameState = GameState.RoundStarted;
        }

        public void PerformTurns()
        {
            foreach (var player in players)
            {
                outputProvider.WriteLine($"Player {player.playerName}'s turn.");
                currentPlayer = player;
                PerformSingleTurn();
            }
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
                    // Solve defaults to 800 pts.
                    Solve(); //Player can only try to solve once, if incorrect, continue guessing letters
                    Console.ReadKey();
                    break;
                default:
                    outputProvider.WriteLine("Invalid option. Please choose 1 to spin or 2 to solve.");
                    GameState = GameState.WaitingForUserInput;
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
            var points = SpinWheel.Spin();

            switch (points)
            {
                case "Bankrupt":
                    Scoreboard.BankruptPlayer(currentPlayer.playerName);
                    break;
                case "Lose a Turn":
                    outputProvider.WriteLine($"Sorry, {currentPlayer.playerName}, you lose a turn.");
                    break;
                default:
                    GuessLetter((int)points);
                    break;
            }
            
        }

        public void Solve()
        {
            outputProvider.Write("Please enter your solution:");
            var guess = inputProvider.Read();
            // Solve defaults to 800 points
            int points = 800;

            //Check if the guess is correct
            if (string.Equals(guess, TemporaryPuzzle, StringComparison.OrdinalIgnoreCase)) 
            {
                charGuessSet.Clear();
                outputProvider.WriteLine($"You are correct! You have solved the puzzle and earned ${points}!");
                Scoreboard.UpdateScore(currentPlayer.playerName, points);
                GameState = GameState.RoundOver; //End the round if the puzzle is solved correctly
                
            }
            else
            {
                outputProvider.WriteLine("Incorrect solution. Press any key to continue.");
                GameState = GameState.GuessingLetter; // Go back to guessing Letters after a wrong solution
            }
        }
        public void GuessLetter(int points)
        {
            GameState = GameState.GuessingLetter;
            outputProvider.Write("Please guess a letter: ");
            var guess = inputProvider.Read();
            bool invalidInput = true;

            while (invalidInput)
            {
                if (string.IsNullOrEmpty(guess) || guess.Length != 1 || !char.IsLetter(guess[0]))
                {
                    outputProvider.WriteLine("Invalid input. Please guess a single letter.");
                    GameState = GameState.GuessingLetter;
                    guess = inputProvider.Read();
                }
                else
                {
                    invalidInput = false;
                }
            }


            CheckGuessedLetter(guess, points);

            HiddenPuzzleDisplay = HidePuzzleSolution(TemporaryPuzzle);
            
        }

        private void CheckGuessedLetter(string guess, int points)
        {
            char guessedLetter = guess[0];

            if (TemporaryPuzzle.Contains(guess) && !charGuessSet.Contains(guessedLetter))
            {
                Scoreboard.UpdateScore(currentPlayer.playerName, points);
                outputProvider.WriteLine($"Good guess! The letter {guess} is in the word. You have earned ${points}!");
            }
            else if (TemporaryPuzzle.Contains(guess) && charGuessSet.Contains(guessedLetter))
            {
                outputProvider.WriteLine($"Sorry, the letter {guess} was already correctly guessed.");
            }
            else
            {
                outputProvider.WriteLine($"Sorry, the letter {guess} is not in the word.");
            }
            charGuessSet.Add(guessedLetter);
            outputProvider.WriteLine("Letters Guessed: " + String.Join(" ", charGuessSet));
        }

        /// <summary>
        /// Optional logic to accept configuration options
        /// </summary>
        public void InitGame()
        {

            outputProvider.WriteLine("Welcome to Wheel of Fortune!");
            GameSetup();
            
            StartNewRound();
        }

        private void PopulateScoreboard()
        {
            foreach (var player in players)
            {
                Scoreboard.AddPlayer(player.playerName);
            }

        }

        public void GameSetup()
        {
            AddPlayer();
            bool addingPlayers = true;

            while (addingPlayers)
            {
                outputProvider.WriteLine("Add another player? (y/n): ");
                GameState = GameState.WaitingForUserInput;

                var action = inputProvider.Read();

                switch (action)
                {
                    case "y":
                        AddPlayer();
                        break;
                    case "n":
                        addingPlayers = false;
                        break;
                    default:
                        outputProvider.WriteLine("Invalid option. Please choose 'y' to add another player or 'n' to proceed.");
                        GameState = GameState.WaitingForUserInput;
                        continue;
                }
            }

            PopulateScoreboard();
            Scoreboard.DisplayScores(); // Scores not displaying
            
        }


        public void AddPlayer()
        {
            outputProvider.WriteLine("Please enter a new player name: ");
            GameState = GameState.WaitingForUserInput;
            var playerName = inputProvider.Read();

            players.Add(new Player(playerName));
            
        }
    }
}

