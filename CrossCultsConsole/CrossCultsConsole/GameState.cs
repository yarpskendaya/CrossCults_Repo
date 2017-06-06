using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCultsConsole
{
    class GameState
    {
        public Player whitePlayer;
        public Player blackPlayer;
        public Board board;

        public GameState(Player w, Player b, Board bo)
        {
            whitePlayer = w;
            blackPlayer = b;
            board = bo;
        }

        //Return the value of this gamestate in terms of who's winning
        public int MiniMax(bool isWhite)
        {
            int minimax;
            if (IsTerminal())
                minimax = Heuristic();
            else
            {
                List<TurnChoice> choices = GetTurnChoices(isWhite);
                List<GameState> successors = GetSuccessors(choices, isWhite);
                int bestScore = successors[0].MiniMax(!isWhite);
                if (isWhite)
                {
                    for (int i = 0; i < successors.Count; i++)
                    {
                        //Find the highest score in the successors for the white player
                        int crntHeuristic = successors[i].MiniMax(!isWhite);
                        Console.WriteLine(crntHeuristic);
                        if (bestScore < crntHeuristic)
                            bestScore = crntHeuristic;
                    }
                    minimax = bestScore;
                }
                else
                {
                    for (int i = 0; i < successors.Count; i++)
                    {
                        //Find the lowest score in the successors for the black player
                        int crntHeuristic = successors[i].MiniMax(!isWhite);
                        Console.WriteLine(crntHeuristic);
                        if (bestScore > crntHeuristic)
                            bestScore = crntHeuristic;
                    }
                    minimax = bestScore;
                }
            }
            return minimax;
        }

        //Get all possible gamestates that can result from this gamestate
        public List<GameState> GetSuccessors(List<TurnChoice> choices, bool isWhite)
        {
            List<GameState> successors = new List<GameState>();
            foreach (TurnChoice tc in choices)
                successors.Add(GetNewGameState(tc));
            return successors;
        }

        //Get all possible moves from this gamestate
        private List<TurnChoice> GetTurnChoices(bool isWhite)
        {
            if (isWhite)
                return whitePlayer.GetValidTurnChoices(blackPlayer.pos);

            else
                return blackPlayer.GetValidTurnChoices(whitePlayer.pos);
        }

        private GameState GetNewGameState(TurnChoice tc)
        {
            GameState successor = new GameState(whitePlayer, blackPlayer, board);
            
            if(tc.isWhite)
            {
                successor.whitePlayer.cards.Remove(tc.actionCard);
                successor.whitePlayer.movement[tc.movementNr - 1] = false;
                successor.whitePlayer.pos = whitePlayer.pos.GetNewPosition(tc.direction, tc.movementNr);
                successor.board.UpdateBoard(tc);
            }

            else
            {
                successor.blackPlayer.cards.Remove(tc.actionCard);
                successor.blackPlayer.movement[tc.movementNr - 1] = false;
                successor.blackPlayer.pos = blackPlayer.pos.GetNewPosition(tc.direction, tc.movementNr);
                successor.board.UpdateBoard(tc);
            }
            return successor;
        }

        //Get a value that represents the gamestate in terms of who is winning
        private int Heuristic()
        {
            int heuristic = 0;

            int tmpl_Score = 10;
            int dcpl_Score = 5;

            //Someone won the game
            if (board.tmpl_B >= 4 && board.tmpl_B > board.tmpl_W)
                heuristic -= 1000;
            if (board.tmpl_W >= 4 && board.tmpl_W > board.tmpl_B)
                heuristic += 1000;

            //The amount of temples
            heuristic -= board.tmpl_B * tmpl_Score;
            heuristic += board.tmpl_W * tmpl_Score;

            //The amount of disciples
            heuristic -= board.tmpl_B * dcpl_Score;
            heuristic += board.tmpl_W * dcpl_Score;

            //The amount of influence
            heuristic -= board.infl_B;
            heuristic += board.infl_W;

            return heuristic;
        }

        //Does the round end in this state?
        private bool IsTerminal()
        {
            //If there are no cards (turns) left
            if (whitePlayer.cards.Count == 0 && blackPlayer.cards.Count == 0)
                return true;
            else
                return false;
        }
    }
}
