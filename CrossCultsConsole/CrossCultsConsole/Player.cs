using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCultsConsole
{
    abstract class Player
    {
        public List<Card> cards = new List<Card>();
        public bool[] movement = new bool[4] { true, true, true, true };
        public Position pos;

        public Player(Position p)
        {
            pos = p;
        }

        public abstract void PickACard(List<Card> availableCards, Board board);

        public abstract TurnChoice GetTurn(Board board);

        protected List<Direction> GetValidDirections(int m, Position otherPos)
        {
            List<Direction> directions = new List<Direction>();
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                //If the destination tile is within bounds and the other player 
                //isn't already standing on it, add it to the list
                Position newPos = pos.GetNewPosition(dir, m);
                if (newPos.WithinBounds() && newPos != otherPos)
                    directions.Add(dir);
            }

            return directions;
        }

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
            TurnChoice turnChoice = new TurnChoice(a, m, dir, pos);
            if (a.type == Card.Type.MerchantsBribe)
            {
                Direction aimDir = GetDirectionChoice(false, m, board);
                turnChoice = new TurnChoice(a, m, dir, pos, aimDir);
            }
            Console.Write("You play {0} with movement {1} going {2}", a.type, m, dir);
            if (a.type == Card.Type.MerchantsBribe) Console.Write(" with aim direction " + turnChoice.aimDirection);
            Console.WriteLine("");
            Console.WriteLine("------------------");
            pos = pos.GetNewPosition(turnChoice.direction, turnChoice.movementNr);
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
            //Do the first thing that comes to mind
            TurnChoice turn = GetValidTurnChoices(board.whitePos)[0];

            //Apply changes of this turn to player variables
            pos = pos.GetNewPosition(turn.direction, turn.movementNr);
            cards.Remove(turn.actionCard);
            movement[turn.movementNr - 1] = false;

            Console.Write("Computer plays {0} with movement {1} going {2}", turn.actionCard.type, turn.movementNr, turn.direction);
            if (turn.actionCard.type == Card.Type.MerchantsBribe) Console.Write(" with aim direction " + turn.aimDirection);
            Console.WriteLine("");

            return turn;
        }

        protected List<TurnChoice> GetValidTurnChoices(Position oPos)
        {
            List<TurnChoice> turns = new List<TurnChoice>();

            for (int i = 0; i < 4; i++)
            {
                if (movement[i])
                {
                    foreach (Direction dir in GetValidDirections(i + 1, oPos))
                    {
                        foreach (Card c in cards)
                        {
                            //Merchants Bribe needs all the directional choices for its aim
                            if (c.type == Card.Type.MerchantsBribe)
                            {
                                foreach (Direction ad in Enum.GetValues(typeof(Direction)))
                                {
                                    turns.Add(new TurnChoice(c, i + 1, dir, pos, ad));
                                }
                            }
                            else
                            {
                                turns.Add(new TurnChoice(c, i + 1, dir, pos));
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Amount of possibilities this turn = " + turns.Count);
            return turns;
        }
    }
}
