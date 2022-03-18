using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        public static void CreateRounds(TournamentModel model)
        {
            List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.EnteredTeams);
            int rounds=FindNumberOfRounds(randomizedTeams.Count);
            int byes = NumberOfByes(rounds, randomizedTeams.Count);
            model.Rounds.Add(CreateFirstRound(byes,randomizedTeams));
            CreateOtherRounds(model,rounds);
        }
        public static void UpdateTournamentResults(TournamentModel model)
        {
            List<MatchupModel> toScore = new List<MatchupModel>();
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel rm in round)
                {
                    if (rm.Winner==null &&(rm.Entries.Any(x=>x.Score!=0)||rm.Entries.Count==1))
                    {
                        toScore.Add(rm);
                    }
                }
            }
            MarkWinnerInMatchups(toScore);
            AdvanceWinners(toScore,model);
            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));        
        }
        private static void AdvanceWinners(List<MatchupModel> matchups, TournamentModel tournament)
        {
            foreach (MatchupModel m in matchups)
            {
                foreach (List<MatchupModel> rounds in tournament.Rounds)
                {
                    foreach (MatchupModel rm in rounds)
                    {
                        foreach (MatchupEntryModel me in rm.Entries)
                        {
                            if (me.ParentMatchup != null)
                            {
                                if (me.ParentMatchup.Id == m.Id)
                                {
                                    me.TeamCompeting = m.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(rm);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void MarkWinnerInMatchups(List<MatchupModel> models)
        {
            foreach (MatchupModel m in models)
            {
                if (m.Entries.Count == 1)
                {
                    m.Winner = m.Entries[0].TeamCompeting;
                    continue;
                }
                if (m.Entries[0].Score > m.Entries[1].Score)
                {
                    m.Winner=m.Entries[0].TeamCompeting;
                }
                else if(m.Entries[0].Score < m.Entries[1].Score)
                {
                    m.Winner = m.Entries[1].TeamCompeting;
                }
                else
                {
                    throw new Exception("we don't allow ties in this application");
                }
            }
        }
        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0];
            List<MatchupModel> currentRound = new List<MatchupModel>();
            MatchupModel currentMatchup=new MatchupModel();
            while (round<=rounds)
            {
                foreach (MatchupModel match in previousRound)
                {
                    currentMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });
                    if (currentMatchup.Entries.Count > 1)
                    {
                        currentMatchup.MatchupRound = round;
                        currentRound.Add(currentMatchup);
                        currentMatchup = new MatchupModel();
                    }
                }
                model.Rounds.Add(currentRound);
                previousRound=currentRound;
                currentRound=new List<MatchupModel>();
                round++;
            }
        }
        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel>output=new List<MatchupModel>();
            MatchupModel curr=new MatchupModel();
            foreach(TeamModel team in teams)
            {
                curr.Entries.Add(new MatchupEntryModel { TeamCompeting=team });
                if (byes>0||curr.Entries.Count>1)
                {
                    curr.MatchupRound = 1;
                    output.Add(curr);
                    curr=new MatchupModel();
                    if (byes>0)
                    {
                        byes -= 1;
                    }
                }
            }
            return output;
        }
        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            int output=0;
            int totalTeams = 1;
            for (int i = 0; i < rounds; i++)
            {
                totalTeams *= 2;
            }
            output=totalTeams-numberOfTeams;
            return output;
        }
        private static int FindNumberOfRounds(int teamCount)
        { 
            int output=1;
            int val = 2;
            while (val<teamCount)
            {
                output += 1;
                val*=2;
            }
            return output;
        }
        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            return teams.OrderBy(x=>Guid.NewGuid()).ToList();
        }
    }
}
