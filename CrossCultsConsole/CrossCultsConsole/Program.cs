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
                    if (Console.ReadLine() == "draw")
                    {
                        board.DrawBoard();
                    }
                    board.UpdateBoard(players[0].GetTurn(board), players[0].pos);
                    board.UpdateBoard(players[1].GetTurn(board), players[1].pos);
                }
                else
                {
                    board.UpdateBoard(players[1].GetTurn(board), players[1].pos);
                    board.UpdateBoard(players[0].GetTurn(board), players[0].pos);
                }
            }
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

    abstract class Player
    {
        public List<Card> cards = new List<Card>();
        public bool[] movement = new bool[4] {true, true, true, true};
        public Position pos;

        public Player (Position p)
        {
            pos = p;
        }

        public abstract void PickACard(List<Card> availableCards, Board board);

        public abstract TurnChoice GetTurn(Board board);

        public void Reset()
        {
            cards = new List<Card>();
            movement = new bool[4] { true, true, true, true };
        }
    }

    class HumanPlayer : Player
    {
        public HumanPlayer(Position pos) : base(pos)
        {
        }

        public override void PickACard(List<Card> availableCards, Board board)
        {
            WriteCardChoice(availableCards);
            int choice = GetIntChoice(0, availableCards.Count - 1);
            Console.WriteLine("You picked: " + availableCards[choice].type);
            Console.WriteLine("------------");
            cards.Add(availableCards[choice]);
            availableCards.RemoveAt(choice);
        }

        //Get the player's turn as a TurnChoice object
        public override TurnChoice GetTurn(Board board)
        {
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("It's your turn, pick an Action Card");
            WriteCardChoice(cards);
            int aChoice = GetIntChoice(0, cards.Count - 1);
            Card a = cards[aChoice];
            Console.WriteLine("You picked: " + cards[aChoice].type);
            cards.RemoveAt(aChoice);
            WriteMovementChoice();
            int m = GetMovementChoice();
            Console.WriteLine("You picked Movement " + m);
            movement[m - 1] = false;
            Direction dir = GetDirectionChoice(true, m, board);
            TurnChoice turnChoice = new TurnChoice(a, m, dir);
            if(a.type == Card.Type.MerchantsBribe)
            {
                Direction aimDir = GetDirectionChoice(false, m, board);
                turnChoice = new TurnChoice(a, m, dir, aimDir);
            }
            Console.Write("You play {0} with movement {1} going {2}", a.type, m, dir);
            if (a.type == Card.Type.MerchantsBribe) Console.Write(" with aim direction " + turnChoice.aimDirection);
            Console.WriteLine("");
            Console.WriteLine("------------------");
            return turnChoice;
        }

        //Returns a valid choice from a human player between first and last available numbers
        int GetIntChoice(int first, int last)
        {
            int choice = 0;
            bool chosen = false;
            while (!chosen)
            {
                string input = Console.ReadLine();
                try { choice = int.Parse(input); }
                catch (FormatException e)
                {
                    Console.WriteLine("Type the number of your choice");
                    continue;
                }
                if (choice < first || choice > last)
                {
                    Console.WriteLine("Choice not valid");
                    continue;
                }
                chosen = true;
            }
            return choice;
        }

        int GetMovementChoice()
        {
            int choice = 0;
            bool chosen = false;
            while (!chosen)
            {
                string input = Console.ReadLine();
                try { choice = int.Parse(input); }
                catch (FormatException e)
                {
                    Console.WriteLine("Type the number of your choice");
                    continue;
                }
                if (choice < 1 || choice > 4)
                {
                    Console.WriteLine("Choice not valid");
                    continue;
                }
                else if (!movement[choice - 1])
                {
                    Console.WriteLine("Card was already used");
                    continue;
                }

                chosen = true;
            }
            return choice;
        }

        Direction GetDirectionChoice(bool check, int movNr, Board board)
        {
            if (check)
                Console.WriteLine("Pick a direction");
            else
                Console.WriteLine("Pick a direction for your merchant");
            Direction dir = Direction.N;
            bool chosen = false;
            while (!chosen)
            {
                string input = Console.ReadLine();
                if (Enum.TryParse<Direction>(input, out dir))
                {
                    if ((CheckNewPosition(this.pos.GetNewPosition(dir, movNr), board)) || !check)
                    {
                        chosen = true;
                        Console.WriteLine("You picked: " + dir);
                    }
                    else
                    {
                        Console.WriteLine("Can't go there");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("Type your direction");
                    continue;
                }
            }
            return dir;
        }

        //Check whether the given movement is legal
        bool CheckNewPosition(Position newPos, Board board)
        {
            //Out Of Bounds
            if (newPos.X > 5 || newPos.Y > 5 || newPos.X < 0 || newPos.Y < 0)
                return false;
            else
            {
                //If either of the monks is already on the destination tile, the other can't move there
                if (board.tiles[newPos.X, newPos.Y].monk_B || board.tiles[newPos.X, newPos.Y].monk_W)
                    return false;
            }
            return true;
        }

        //Display available cards for the player
        void WriteCardChoice(List<Card> cards)
        {
            Console.WriteLine("Pick a card:");
            for (int i = 0; i < cards.Count; i++)
            {
                Console.WriteLine("{0}. {1}", i, cards[i].type);
            }
        }

        //Display available movement cards for the player
        void WriteMovementChoice()
        {
            Console.WriteLine("Pick a Movement Card");
            for (int i = 1; i <= 4; i++)
            {
                if (movement[i - 1])
                    Console.WriteLine("Movement Card " + i);
            }
        }
    }

    class ComputerPlayer : Player
    {
        public ComputerPlayer(Position pos) : base(pos)
        {
        }

        public override void PickACard(List<Card> availableCards, Board board)
        {
            //TODO: Add AI
            int choice = 0;
            Console.WriteLine("Computer picked: " + availableCards[choice].type);
            cards.Add(availableCards[choice]);
            availableCards.RemoveAt(choice);
        }

        public override TurnChoice GetTurn(Board board)
        {
            //TODO: Add AI
            Console.WriteLine("Get the computers turn");
            return new TurnChoice(new Card(Card.Type.ActOfViolence), 3, Direction.N);
        }
    }

    //Object to define a turn with
    class TurnChoice
    {
        public Card actionCard;
        public int movementNr;
        public Direction direction;
        public Direction aimDirection;

        public TurnChoice(Card a, int m, Direction dir, Direction adir = Direction.N)
        {
            actionCard = a;
            movementNr = m;
            direction = dir;
            aimDirection = adir;
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

    class Board
    {
        public int nrOfTiles;
        public Tile[,] tiles;

        public int infl_W = 0; public int infl_B = 0;
        public int tmpl_W = 0; public int tmpl_B = 0;
        public int dcpl_W = 0; public int dcpl_B = 0;

        public Board(int t)
        {
            nrOfTiles = t;
            tiles = new Tile[nrOfTiles, nrOfTiles];
            for (int x = 0; x < nrOfTiles; x++)
                for (int y = 0; y < nrOfTiles; y++)
                    tiles[x, y] = new Tile(new Position(x, y));
        }

        public void SetUpInitialBoard()
        {
            foreach (Tile t in tiles)
                t.Reset();

            tiles[1, 1].monk_W = true;
            tiles[4, 4].monk_B = true;
            tiles[1, 1].infl_W += 1;
            tiles[4, 4].infl_B += 1;
        }

        //Get the next turn and play its effects out on the board
        public void UpdateBoard(TurnChoice turn, Position curPos)
        {
            bool isWhite = true;
            Position newPos = curPos;

            //If it's the white monk
            if (tiles[curPos.X, curPos.Y].monk_W)
            {
                tiles[curPos.X, curPos.Y].monk_W = false;
                newPos = curPos.GetNewPosition(turn.direction, turn.movementNr);
                tiles[newPos.X, newPos.Y].monk_W = true;
            }
            //If it's the black monk
            else if (tiles[curPos.X, curPos.Y].monk_B)
            {
                isWhite = false;
                tiles[curPos.X, curPos.Y].monk_B = false;
                newPos = curPos.GetNewPosition(turn.direction, turn.movementNr);
                tiles[newPos.X, newPos.Y].monk_B = true;
            }

            else Console.WriteLine("ERROR: Player was not at starting position");

            switch (turn.actionCard.type)
            {
                case Card.Type.SpreadInfluence:
                    DoSpreadInfluence(isWhite, newPos);
                    break;
                case Card.Type.PathOfFaith:
                    DoPathOfFaith(isWhite, curPos, newPos);
                    break;
                case Card.Type.Disciple:
                    DoDisciple(isWhite, newPos);
                    break;
                case Card.Type.ActOfViolence:
                    DoActOfViolence(isWhite, newPos);
                    break;
                case Card.Type.PreachDistrust:
                    DoPreachDistrust(isWhite, newPos, turn.direction);
                    break;
                case Card.Type.MerchantsBribe:
                    DoMerchantsBribe(isWhite, newPos, turn.aimDirection);
                    break;
                default:
                    break;
            }
            UpdateCounters();
        }

        public void DoSpreadInfluence(bool isWhite, Position dest)
        {
            //List of positions that get 1 new influence
            List<Position> positions = new List<Position>();

            //Monk spreads influence
            positions.Add(dest);
            if ((tiles[dest.X, dest.Y].infl_W > 0 && isWhite) || (tiles[dest.X, dest.Y].infl_B > 0 && !isWhite))
            {
                positions.Add(dest + new Position(-1, 0));
                positions.Add(dest + new Position(0, 1));
                positions.Add(dest + new Position(1, 0));
                positions.Add(dest + new Position(0, -1));
            }
            //Disciples spread influence
            foreach (Tile t in tiles)
            {
                if((isWhite && t.dcpl_W && t.infl_W > 0) || (!isWhite && t.dcpl_B && t.infl_B > 0))
                {
                    if (!positions.Contains(t.pos)) positions.Add(t.pos);
                    Position adjacent = t.pos + new Position(-1, 0);
                    if (!positions.Contains(adjacent)) positions.Add(adjacent);
                    adjacent = t.pos + new Position(0, 1);
                    if (!positions.Contains(adjacent)) positions.Add(adjacent);
                    adjacent = t.pos + new Position(1, 0);
                    if (!positions.Contains(adjacent)) positions.Add(adjacent);
                    adjacent = t.pos + new Position(0, -1);
                    if (!positions.Contains(adjacent)) positions.Add(adjacent);
                }
            }

            foreach (Position p in positions)
            {
                tiles[p.X, p.Y].AddInfluence(isWhite);
            }
        }

        public void DoPathOfFaith(bool isWhite, Position start, Position dest)
        {
            //Get an increment position to define each tile that is traversed this turn
            Position increment = dest - start;
            if (increment.X < 0) increment.X = -1;
            if (increment.X > 0) increment.X = 1;
            if (increment.Y < 0) increment.Y = -1;
            if (increment.Y > 0) increment.Y = 1;

            while (start != dest)
            {
                start += increment;
                tiles[start.X, start.Y].AddInfluence(isWhite);
            }
        }

        public void DoDisciple(bool isWhite, Position dest)
        {
            if (dcpl_W < 2)
            {
                tiles[dest.X, dest.Y].AddDisciple(isWhite);
            }
        }

        public void DoActOfViolence(bool isWhite, Position dest)
        {
            tiles[dest.X, dest.Y].RemoveDisciple(isWhite);
        }

        public void DoPreachDistrust(bool isWhite, Position dest, Direction dir)
        {
            Position[] targets = new Position[3];

            switch (dir)
            {
                case Direction.N:
                    targets[1] = dest + new Position(-1, 1);
                    targets[2] = dest + new Position(0, 1);
                    targets[3] = dest + new Position(1, 1);
                    break;
                case Direction.E:
                    targets[1] = dest + new Position(1, 1);
                    targets[2] = dest + new Position(1, 0);
                    targets[3] = dest + new Position(1, -1);
                    break;
                case Direction.S:
                    targets[1] = dest + new Position(-1, -1);
                    targets[2] = dest + new Position(0, -1);
                    targets[3] = dest + new Position(1, -1);
                    break;
                case Direction.W:
                    targets[1] = dest + new Position(-1, -1);
                    targets[2] = dest + new Position(-1, 0);
                    targets[3] = dest + new Position(-1, 1);
                    break;
                case Direction.NE:
                    targets[1] = dest + new Position(0, 1);
                    targets[2] = dest + new Position(1, 1);
                    targets[3] = dest + new Position(1, 0);
                    break;
                case Direction.SE:
                    targets[1] = dest + new Position(1, 0);
                    targets[2] = dest + new Position(1, -1);
                    targets[3] = dest + new Position(0, -1);
                    break;
                case Direction.SW:
                    targets[1] = dest + new Position(0, -1);
                    targets[2] = dest + new Position(-1, -1);
                    targets[3] = dest + new Position(-1, 0);
                    break;
                case Direction.NW:
                    targets[1] = dest + new Position(-1, 0);
                    targets[2] = dest + new Position(-1, 1);
                    targets[3] = dest + new Position(0, 1);
                    break;
                default:
                    break;

                    foreach (Position p in targets)
                    {
                        //If it's not out of bounds
                        if (p.X >= 0 && p.X <= 5 && p.Y >= 0 && p.Y <= 5)
                        {
                            tiles[p.X, p.Y].RemoveInfluence(isWhite, 2);
                        }
                    }
            }
        }

        public void DoMerchantsBribe(bool isWhite, Position dest,  Direction aimDir)
        {
            Position targetPos = dest;
            Position posInc = targetPos.GetPositionIncrement(aimDir);
            while (targetPos.X >= 0 && targetPos.X <= 5 && targetPos.Y >= 0 && targetPos.Y <= 5)
            {
                targetPos += posInc;
                tiles[targetPos.X, targetPos.Y].RemoveInfluence(isWhite, 1);
            }
        }

        public void DrawBoard()
        {
            for (int x = 0; x < nrOfTiles; x++)
            {
                Console.WriteLine("Row" + x + " I, D, T, M");
                for (int y = 0; y < nrOfTiles; y++)
                {
                    char D = '-';
                    if (tiles[x, y].dcpl_W) D = 'W';
                    if (tiles[x, y].dcpl_B) D = 'B';
                    char T = '-';
                    if (tiles[x, y].tmpl_W) T = 'W';
                    if (tiles[x, y].tmpl_B) T = 'B';
                    char M = '-';
                    if (tiles[x, y].monk_W) M = 'W';
                    if (tiles[x, y].monk_B) M = 'B';

                    Console.WriteLine("{0},{1} {2}/{3}, {4}, {5}, {6} ", 
                        x, y, tiles[x,y].infl_W, tiles[x,y].infl_B, D, T, M);
                }
                Console.WriteLine(" ");
            }
        }

        //Update the total amount of influence, temples and disciples on the board
        void UpdateCounters()
        {
            infl_B = 0; infl_W = 0;
            tmpl_B = 0; tmpl_W = 0;
            dcpl_B = 0; dcpl_W = 0;

            foreach (Tile t in tiles)
            {
                infl_B += t.infl_B; infl_W += t.infl_W;
                if (t.tmpl_B) tmpl_B += 1; if (t.tmpl_W) tmpl_W += 1;
                if (t.dcpl_B) dcpl_B += 1; if (t.dcpl_W) dcpl_W += 1;
            }
        }
    }

    class Tile
    {
        public int infl_W = 0; public int infl_B = 0;
        public bool tmpl_W = false; public bool tmpl_B = false;
        public bool monk_W = false; public bool monk_B = false;
        public bool dcpl_W = false; public bool dcpl_B = false;

        public Position pos = new Position(0, 0);

        public Tile(Position p)
        {
            pos = p;
        }

        public void AddInfluence(bool isWhite)
        {
            if (isWhite)
            {
                if (infl_W < 3 && !tmpl_W && !tmpl_B) infl_W += 1;
                else if (infl_W == 3)
                {
                    infl_W = 0;
                    dcpl_B = false;
                    dcpl_W = false;
                    tmpl_W = true;
                }
            }

            else
            {
                if (infl_B < 3 && !tmpl_W && !tmpl_B) infl_B += 1;
                else if (infl_B == 3)
                {
                    infl_B = 0;
                    tmpl_B = true;
                }
            }
        }

        public void RemoveInfluence(bool isWhite, int amount)
        {
            if (isWhite)
            {
                infl_B -= amount;
                if (infl_B < 0) infl_B = 0;
            }
            else
            {
                infl_W -= amount;
                if (infl_W < 0) infl_W = 0;
            }

        }

        public void AddDisciple(bool isWhite)
        {
            if (!(tmpl_W || tmpl_B || dcpl_W || dcpl_B))
            {
                if (isWhite) dcpl_W = true;
                else dcpl_B = true;
            }
        }

        public void RemoveDisciple(bool isWhite)
        {
            if (isWhite)
            {
                if (dcpl_B)
                {
                    dcpl_B = false;
                    RemoveInfluence(isWhite, 2);
                }
            }
            if (!isWhite)
            {
                if (dcpl_W)
                {
                    dcpl_W = false;
                    RemoveInfluence(isWhite, 2);
                }
            }
        }

        public void Reset()
        {
            infl_W = 0; infl_B = 0;
            tmpl_W = false; tmpl_B = false;
            monk_W = false; monk_B = false;
            dcpl_W = false; dcpl_B = false;
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

            if (otherPos == null)
                return false;
            if (this.X == otherPos.X && this.Y == otherPos.Y)
                return true;
            else return false;
        }
    }
}
