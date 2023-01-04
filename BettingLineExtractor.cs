using CFBSharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    // A class that should extract data from the GameLines class
    public class BettingLineExtractor
    {

        public (int wins, int losses, int ties) GetSpreadWinLoss(string team, ICollection<GameLines> gameLines) 
        {
            int wins = 0;
            int losses = 0;
            int ties = 0;

            foreach (var line in gameLines)
            {
                bool isHomeTeam = line.HomeTeam == team ? true : false;
                decimal? rawBovadaLine = line.Lines
                                        .Where(t => t.Provider == "Bovada")
                                        .Select(s => s.Spread).FirstOrDefault();

                decimal? adjustedScoreVsSpread = GetAdjustedScore(isHomeTeam, line.HomeScore, line.AwayScore, rawBovadaLine);

                switch (adjustedScoreVsSpread) 
                {
                    case > 0:
                        wins++;
                        break;

                    case < 0:
                        losses++;
                        break;
                    default:
                        ties++;
                        break;
                }
            }
            return (wins, losses, ties);
        }

        public decimal? GetSpreadDifferential(string team, ICollection<GameLines> gameLines)
        {
            decimal? differential = 0;

            foreach (var line in gameLines)
            {
                var winner = line.HomeScore - line.AwayScore > 0 ? line.HomeTeam : line.AwayTeam;
                bool teamWin = team == winner ? true : false;
                bool isHomeTeam = line.HomeTeam == team ? true : false;
                decimal? rawBovadaLine = line.Lines
                                        .Where(t => t.Provider == "Bovada")
                                        .Select(s => s.Spread).FirstOrDefault();
                if (rawBovadaLine == null)
                {
                    rawBovadaLine = line.Lines.Select(s => s.Spread).FirstOrDefault();
                }

                decimal? adjustedScore = GetAdjustedScore(isHomeTeam, line.HomeScore, line.AwayScore, rawBovadaLine);

                if (adjustedScore != null)
                {
                    differential += adjustedScore;
                }
                
            }
            return differential;
        }

        private decimal? GetAdjustedScore(bool isHomeTeam, int? homeScore, int? awayScore, decimal? line)
        {
            decimal? adjustedScore;
            decimal? adjustedLine;

            if (isHomeTeam)
            {
                adjustedScore = homeScore - awayScore;
                adjustedLine = line;
            }
            else
            {
                adjustedScore = awayScore - homeScore;
                adjustedLine = line * -1;
            }

            return adjustedScore + adjustedLine;
        }
    }
}
