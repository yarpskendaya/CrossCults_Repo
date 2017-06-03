using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCultsConsole
{
    class GameState
    {
        Player whitePlayer;
        Player blackPlayer;
        Board board;

        //Return the value of this gamestate in terms of who's winning
        public float MiniMax(bool isWhite)
        {
            float minimax;
            if (IsTerminal())
                minimax = Heuristic();
            else
            {
                List<TurnChoice> choices = GetTurnChoices(isWhite);
                List<GameState> successors = GetSuccessors(choices, isWhite);
                GameState bestSuccessor = successors[0];
                int best = 0;
                if (isWhite)
                {
                    for (int i = 0; i < successors.Count; i++)
                    {
                        //Find the highest score in the successors for the white player
                        if (bestSuccessor.MiniMax(!isWhite) < successors[i].MiniMax(!isWhite))
                        {
                            bestSuccessor = successors[i];
                            best = i;
                        }
                            
                    }
                }
                else
                {
                    for (int i = 0; i < successors.Count; i++)
                    {
                        //Find the highest score in the successors for the black player
                        if (bestSuccessor.MiniMax(!isWhite) > successors[i].MiniMax(!isWhite))
                        {
                            bestSuccessor = successors[i];
                            best = i;
                        } 
                    }
                }
            }
            return minimax;
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

        //Get all possible gamestates that can result from this gamestate
        private List<GameState> GetSuccessors(List<TurnChoice> choices, bool isWhite)
        {
            List<GameState> successors = new List<GameState>();
            foreach (TurnChoice tc in choices)
                successors.Add(GetNewGameState(tc, isWhite));
            return successors;
        }

        //Get all possible moves from this gamestate
        private List<TurnChoice> GetTurnChoices(bool isWhite)
        {
            List<TurnChoice> choices = new List<TurnChoice>();
            //TODO
            return choices;
        }

        private GameState GetNewGameState(TurnChoice tc, bool isWhite)
        {
            GameState successor = this;
            //TODO
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
    }
}
