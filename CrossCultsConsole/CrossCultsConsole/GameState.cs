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

        public float MiniMax(bool isWhite)
        {
            float minimax;
            if (IsTerminal()) return 
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

        private int Heuristic()
        {
            int heuristic = 0;
            int tmpl_Score = 10;


            //Someone won the game
            if (board.tmpl_B >= 4 && board.tmpl_B > board.tmpl_W)
                heuristic -= 1000;
            if (board.tmpl_W >= 4 && board.tmpl_W > board.tmpl_B)
                heuristic += 1000;

            //The amount of temples
            heuristic -= board.tmpl_B * tmpl_Score;
            heuristic += board.tmpl_W * tmpl_Score;

            //The amount of influence
            
        }
    }
}
