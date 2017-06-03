using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCultsConsole
{
    class Board
    {
        public int nrOfTiles;
        public Tile[,] tiles;
        public Position whitePos;
        public Position blackPos;
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
            whitePos = new Position(1, 1);
            blackPos = new Position(4, 4);
            tiles[1, 1].infl_W += 1;
            tiles[4, 4].infl_B += 1;
        }

        //Get the next turn and play its effects out on the board
        public void UpdateBoard(TurnChoice turn)
        {
            bool isWhite = true;
            Position newPos = turn.startPos;

            //If it's the white monk
            if (tiles[turn.startPos.X, turn.startPos.Y].monk_W)
            {
                tiles[turn.startPos.X, turn.startPos.Y].monk_W = false;
                newPos = turn.startPos.GetNewPosition(turn.direction, turn.movementNr);
                tiles[newPos.X, newPos.Y].monk_W = true;
                whitePos = newPos;
            }
            //If it's the black monk
            else if (tiles[turn.startPos.X, turn.startPos.Y].monk_B)
            {
                isWhite = false;
                tiles[turn.startPos.X, turn.startPos.Y].monk_B = false;
                newPos = turn.startPos.GetNewPosition(turn.direction, turn.movementNr);
                tiles[newPos.X, newPos.Y].monk_B = true;
                blackPos = newPos;
            }

            else Console.WriteLine("ERROR: Player was not at starting position");

            switch (turn.actionCard.type)
            {
                case Card.Type.SpreadInfluence:
                    DoSpreadInfluence(isWhite, newPos);
                    break;
                case Card.Type.PathOfFaith:
                    DoPathOfFaith(isWhite, turn.startPos, newPos);
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
                if ((isWhite && t.dcpl_W && t.infl_W > 0) || (!isWhite && t.dcpl_B && t.infl_B > 0))
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
                    targets[0] = dest + new Position(-1, 1);
                    targets[1] = dest + new Position(0, 1);
                    targets[2] = dest + new Position(1, 1);
                    break;
                case Direction.E:
                    targets[0] = dest + new Position(1, 1);
                    targets[1] = dest + new Position(1, 0);
                    targets[2] = dest + new Position(1, -1);
                    break;
                case Direction.S:
                    targets[0] = dest + new Position(-1, -1);
                    targets[1] = dest + new Position(0, -1);
                    targets[2] = dest + new Position(1, -1);
                    break;
                case Direction.W:
                    targets[0] = dest + new Position(-1, -1);
                    targets[1] = dest + new Position(-1, 0);
                    targets[2] = dest + new Position(-1, 1);
                    break;
                case Direction.NE:
                    targets[0] = dest + new Position(0, 1);
                    targets[1] = dest + new Position(1, 1);
                    targets[2] = dest + new Position(1, 0);
                    break;
                case Direction.SE:
                    targets[0] = dest + new Position(1, 0);
                    targets[1] = dest + new Position(1, -1);
                    targets[2] = dest + new Position(0, -1);
                    break;
                case Direction.SW:
                    targets[0] = dest + new Position(0, -1);
                    targets[1] = dest + new Position(-1, -1);
                    targets[2] = dest + new Position(-1, 0);
                    break;
                case Direction.NW:
                    targets[0] = dest + new Position(-1, 0);
                    targets[1] = dest + new Position(-1, 1);
                    targets[2] = dest + new Position(0, 1);
                    break;
                default:
                    break;
            }
            foreach (Position p in targets)
            {
                //If it's not out of bounds
                if (p.X >= 0 && p.X <= 5 && p.Y >= 0 && p.Y <= 5)
                {
                    tiles[p.X, p.Y].RemoveInfluence(isWhite, 2);
                }
            }
        }

        public void DoMerchantsBribe(bool isWhite, Position dest, Direction aimDir)
        {
            Position targetPos = dest;
            Position posInc = targetPos.GetPositionIncrement(aimDir);
            targetPos += posInc;
            while (targetPos.X >= 0 && targetPos.X <= 5 && targetPos.Y >= 0 && targetPos.Y <= 5)
            {
                if (targetPos.X >= 0 && targetPos.X <= 5 && targetPos.Y >= 0 && targetPos.Y <= 5)
                    tiles[targetPos.X, targetPos.Y].RemoveInfluence(isWhite, 1);
                targetPos += posInc;
            }
        }

        public void DrawBoard()
        {
            Console.WriteLine("+------+------+------+------+------+------+");
            for (int y = 5; y >= 0; y--)
            {
                //Write a line for all the Influence in an entire row first
                Console.Write("|");
                for (int x = 0; x < nrOfTiles; x++)
                {
                    if (tiles[x, y].infl_W > 0) Console.Write(tiles[x, y].infl_W);
                    else Console.Write(" ");
                    Console.Write("    ");
                    if (tiles[x, y].infl_B > 0) Console.Write(tiles[x, y].infl_B);
                    else Console.Write(" ");
                    Console.Write("|");
                }
                Console.WriteLine("");

                //Next, go over the row again and write Temples, Monks and Disciples
                Console.Write("|");
                for (int x = 0; x < nrOfTiles; x++)
                {
                    //White temples or disciples
                    if (tiles[x, y].dcpl_W) Console.Write("D ");
                    else if (tiles[x, y].tmpl_W) Console.Write("T ");
                    else Console.Write("  ");
                    //White monk are black monk
                    if (tiles[x, y].monk_W) Console.Write("WM ");
                    else if (tiles[x, y].monk_B) Console.Write("BM ");
                    else Console.Write("   ");
                    //Black temples or disciples
                    if (tiles[x, y].dcpl_B) Console.Write("D");
                    else if (tiles[x, y].tmpl_B) Console.Write("T");
                    else Console.Write(" ");
                    Console.Write("|");
                }
                Console.WriteLine("");
                Console.WriteLine("+------+------+------+------+------+------+");
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
}
