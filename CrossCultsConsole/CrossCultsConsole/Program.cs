using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCultsConsole
{
    enum Direction { N, E, S, W, NE, SE, SW, NW};

    static class Program
    {
        static void Main(string[] args)
        {
            GameManager gameManager = new GameManager();

            Console.WriteLine("Welcome to Cross Cults Console");
            while (true)
            {
                Console.WriteLine("Do you want to play a game against me? (Y/N)");
                string line = Console.ReadLine();
                switch (line)
                {
                    case "Y":
                        gameManager.PlayGame();
                        break;
                    case "N":
                        Console.WriteLine("That's a shame");
                        break;
                    default:
                        Console.WriteLine("I didnt't understand");
                        break;
                }
            }
        }

        //This implements a useful way to shuffle lists (for the cards)
        private static Random rng = new Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    class GameManager
    {
        static int nrOfTiles = 6;
        Board board = new Board(nrOfTiles);
        List<Card> allCards;
        Player human;
        Player computer;
        Player[] players;
        
        public GameManager()
        {
            Player human = new HumanPlayer(new Position(1, 1));
            Player computer = new ComputerPlayer(new Position(4, 4));
            players = new Player[2] { human, computer };
        }

        public void PlayGame()
        {
            Console.WriteLine("Started a new game");
            bool GameCompleted = false;
            bool whiteFirst = true;

            SetUpNewGame();

            while (!GameCompleted)
            {
                PlayRound(whiteFirst);
                whiteFirst = !whiteFirst;
                //TODO: Add logic: Game Completed
            }
        }

        void PlayRound(bool isWhiteFirst)
        {
            //The players choose their cards
            HandleCardChoosing(isWhiteFirst);
            //The players play 4 turns each
            for (int i = 0; i < 4; i++)
            {
                if (isWhiteFirst)
                {
                    board.UpdateBoard(players[0].GetTurn(board));
                    board.DrawBoard();
                    board.UpdateBoard(players[1].GetTurn(board));
                    board.DrawBoard();
                }
                else
                {
                    board.UpdateBoard(players[1].GetTurn(board));
                    board.DrawBoard();
                    board.UpdateBoard(players[0].GetTurn(board));
                    board.DrawBoard();
                }
            }
            players[0].Reset();
            players[1].Reset();
        }

        //Handle the initial card choosing in a round
        void HandleCardChoosing(bool isWhiteFirst)
        {
            //Draw the 9 cards to choose from
            List<Card> cards = new List<Card>();
            cards.AddRange(allCards);
            cards.Shuffle();
            cards.RemoveRange(9, 3);
            //Let the players choose until they both have 4 cards
            for (int i = 0; i < 4; i++)
            {
                if (isWhiteFirst)
                {
                    players[1].PickACard(cards, board);
                    players[0].PickACard(cards, board);
                }
                else
                {
                    players[0].PickACard(cards, board);
                    players[1].PickACard(cards, board);
                }
            }
        }

        void WriteCardChoice(List<Card> cards)
        {
            Console.WriteLine("Pick a card:");
            for (int i = 0; i < cards.Count; i++)
            {
                Console.WriteLine("{0}. {1}", i, cards[i].type);
            }
        }

        //Returns whether the destination tile is available to move to
        bool CanMove (int destX, int destY)
        {
            //Out of bounds
            if (destX >= nrOfTiles || destY >= nrOfTiles || destX < 0 || destY < 0)
                return false;
            //Monk already there
            if (board.tiles[destX, destY].monk_B || board.tiles[destX, destY].monk_W)
                return false;

            else return true;
        }

        public void SetUpNewGame()
        {
            board.SetUpInitialBoard();
            CreateCardList();
            human = new HumanPlayer(new Position(1, 1));
            computer = new ComputerPlayer(new Position(4, 4));
        }

        void CreateCardList()
        {
            allCards = new List<Card>();
            allCards.Add(new Card(Card.Type.SpreadInfluence));
            allCards.Add(new Card(Card.Type.SpreadInfluence));
            allCards.Add(new Card(Card.Type.SpreadInfluence));
            allCards.Add(new Card(Card.Type.Disciple));
            allCards.Add(new Card(Card.Type.Disciple));
            allCards.Add(new Card(Card.Type.PathOfFaith));
            allCards.Add(new Card(Card.Type.PathOfFaith));
            allCards.Add(new Card(Card.Type.ActOfViolence));
            allCards.Add(new Card(Card.Type.ActOfViolence));
            allCards.Add(new Card(Card.Type.MerchantsBribe));
            allCards.Add(new Card(Card.Type.MerchantsBribe));
            allCards.Add(new Card(Card.Type.PreachDistrust));
        }
    }

    //Object to define a turn with
    class TurnChoice
    {
        public Card actionCard;
        public int movementNr;
        public Direction direction;
        public Direction aimDirection;
        public Position startPos;

        public TurnChoice(Card a, int m, Direction dir, Position s, Direction adir = Direction.N)
        {
            actionCard = a;
            movementNr = m;
            direction = dir;
            aimDirection = adir;
            startPos = s;
        }
    }

    class Card
    {
        public enum Type {SpreadInfluence, Disciple, PathOfFaith, ActOfViolence, PreachDistrust, MerchantsBribe};
        public Type type;

        public Card(Type t)
        {
            type = t;
        }
    }

    class Position
    {
        public int X;
        public int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        //Returns the new coordinates (unchecked)
        public Position GetNewPosition(Direction dir, int movement)
        {
            Position posInc = GetPositionIncrement(dir);
            Position newPos = new Position(this.X + posInc.X * movement, this.Y + posInc.Y * movement);
            return newPos;
        }

        //Get normalized vector of direction
        public Position GetPositionIncrement(Direction dir)
        {
            Position posIncrement = new Position(0, 0);
            switch (dir)
            {
                case Direction.N:
                    posIncrement = new Position(0, 1);
                    break;
                case Direction.E:
                    posIncrement = new Position(1, 0);
                    break;
                case Direction.S:
                    posIncrement = new Position(0, -1);
                    break;
                case Direction.W:
                    posIncrement = new Position(-1, 0);
                    break;
                case Direction.NE:
                    posIncrement = new Position(1, 1);
                    break;
                case Direction.SE:
                    posIncrement = new Position(1, -1);
                    break;
                case Direction.SW:
                    posIncrement = new Position(-1, -1);
                    break;
                case Direction.NW:
                    posIncrement = new Position(-1, 1);
                    break;
                default:
                    break;
            }
            return posIncrement;
        }

        //Check if the new position does not go off the board
        public bool WithinBounds()
        {
            bool withinbounds = true;
            if (X < 0 || X > 5 || Y < 0 || Y > 5)
                withinbounds = false;
            return withinbounds;
        }

        //Allows addition of Positions
        public static Position operator +(Position p1, Position p2)
        {
            return new Position(p1.X + p2.X, p1.Y + p2.Y);
        }

        //Allows addition of Positions
        public static Position operator -(Position p1, Position p2)
        {
            return new Position(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static bool operator ==(Position p1, Position p2)
        {
            if (p1.X == p2.X && p1.Y == p2.Y)
                return true;
            else return false;
        }

        public static bool operator !=(Position p1, Position p2)
        {
            if (p1.X != p2.X || p1.Y != p2.Y)
                return true;
            else return false;
        }

        public override bool Equals(object obj)
        {
            var otherPos = obj as Position;

            if (this.X == otherPos.X && this.Y == otherPos.Y)
                return true;
            else return false;
        }
    }
}
