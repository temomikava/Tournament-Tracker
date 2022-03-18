using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    //Load the text file
    //Convert the text to List<model>
    //Find the max ID
    //Add the new record with the new ID(ID=max+1)
    //Convert the model to list<string>
    //Save the list<string> to the text file
    public static class TextConnectorProcessor
    {             
        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }
            return File.ReadAllLines(file).ToList();
        }
        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            //id,placeNumber,placeName,prizeAmount,prizePercentage
            List<PrizeModel> output = new List<PrizeModel>();
            foreach (string line in lines)
            {
                string[] columns=line.Split(',');
                PrizeModel prizeModel = new PrizeModel();
                prizeModel.Id = int.Parse(columns[0]);
                prizeModel.PlaceNumber=int.Parse(columns[1]);
                prizeModel.PlaceName= columns[2];
                prizeModel.PrizeAmount=decimal.Parse(columns[3]);
                prizeModel.PrizePercentage = double.Parse(columns[4]);
                output.Add(prizeModel);
            }
            return output;
        }
        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            //id,firstName,lastName,emailAddress,cellphoneNumber
            List<PersonModel> output = new List<PersonModel>();
            foreach (string line in lines)
            {
                string[] columns = line.Split(',');
                PersonModel person = new PersonModel();
                person.Id = int.Parse(columns[0]);
                person.FirstName = columns[1];
                person.LastName = columns[2];
                person.EmailAddress = columns[3];
                person.CellphoneNumber = columns[4];
                output.Add(person);
            }
            return output;
        }
        public static List<TeamModel>ConvertToTeamModel(this List<string> lines)
        {
            //id,teamname,id|id|id-personModelIds
            List<TeamModel> output=new List<TeamModel>();
            List<PersonModel> persons = TextConnector.PersonsPath.LoadFile().ConvertToPersonModels();

            foreach (string line in lines)
            {
                string[] columns = line.Split(',');
                TeamModel t=new TeamModel();
                t.Id = int.Parse(columns[0]);
                t.TeamName = columns[1];
                string[] personIds = columns[2].Split('|');
                foreach (string id in personIds)
                {
                    if (id != "")
                    {
                       t.TeamMembers.Add(persons.Where(x => x.Id == int.Parse(id)).First());
                    }
                }
                output.Add(t);
            }
            return output;
        }
        public static List<TournamentModel> ConvertToTournamentModel(this List<string> lines)
        {
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams = TextConnector.TeamsPath.LoadFile().ConvertToTeamModel();
            List<PrizeModel> Prizes =TextConnector.PrizesPath.LoadFile().ConvertToPrizeModels();
            List<MatchupModel> matchups = TextConnector.MatchupsPath.LoadFile().ConvertToMatchupModel();


            //input: id,TournamentName,EntryFee,id|id|id-entered teams,id|id|id-prizes,id^id^id|id^id^id^|id^id^id-rounds

            foreach (string line in lines)
            {
                string[] columns = line.Split(',');
                TournamentModel tm=new TournamentModel();
                tm.Id = int.Parse(columns[0]);
                tm.TournamentName = columns[1];
                tm.EntryFee = decimal.Parse(columns[2]);

                string[] teamIds = columns[3].Split('|');
                foreach (string id in teamIds)
                {
                    tm.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(id)).First());
                }
                string[] prizeIds=columns[4].Split('|');
                foreach(string id in prizeIds)
                {
                    if (id.Length!=0)
                    {
                        tm.Prizes.Add(Prizes.Where(x => x.Id == int.Parse(id)).First());
                    }

                }
                // capture Rounds information
                string[]rounds=columns[5].Split('|');
                foreach (string round in rounds)
                {
                    List<MatchupModel> ms = new List<MatchupModel>();
                    string[] msText = round.Split('^');
                    foreach (string matchupModelTextId in msText)
                    {
                        ms.Add(matchups.Where(x => x.Id == int.Parse(matchupModelTextId)).First());
                    }
                    tm.Rounds.Add(ms);
                }
                output.Add(tm);
            }
            return output;
        }
        public static void SaveToPrizesFile(this List<PrizeModel> models, string path)
        {
            List<string>lines=new List<string>();
            foreach (PrizeModel p in models)
            {
                lines.Add($"{p.Id },{ p.PlaceNumber },{ p.PlaceName },{ p.PrizeAmount },{ p.PrizePercentage}");
            }
            File.WriteAllLines(TextConnector.PrizesPath, lines);
        }
        public static void SaveToPersonsFile(this List<PersonModel> models, string path)
        {
            List<string> lines = new List<string>();
            foreach (PersonModel p in models)
            {
                lines.Add($"{p.Id },{ p.FirstName },{ p.LastName },{ p.EmailAddress },{ p.CellphoneNumber}");
            }
            File.WriteAllLines(TextConnector.PersonsPath, lines);
        }
        public static void SaveToTeamsFile(this List<TeamModel> models, string path)
        {
            List<string> lines = new List<string>();
            foreach(TeamModel t in models)
            {
                
                lines.Add($"{ t.Id },{ t.TeamName },{ConvertPersonListToString(t.TeamMembers)}");              
            }
            File.WriteAllLines(TextConnector.TeamsPath, lines);
        }
        public static void UpdateMatchupToFile(this MatchupModel matchup)
        {
            List<MatchupModel> matchups = TextConnector.MatchupsPath.LoadFile().ConvertToMatchupModel();
            MatchupModel oldMatchup = new MatchupModel();
            foreach (MatchupModel m in matchups)
            {
                if (m.Id==matchup.Id)
                {
                    oldMatchup = m;
                }
            }
            matchups.Remove(oldMatchup);
            matchups.Add(matchup);
            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.UpdateMatchupEntriesFile();
            }
            List<string> lines = new List<string>();
            foreach (MatchupModel m in matchups)
            {
                string winner = "";
                if (m.Winner != null)
                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{m.Id },{ ConvertMatchupEntryListToString(m.Entries) },{ winner },{ m.MatchupRound }");
            }
            File.WriteAllLines(TextConnector.MatchupsPath, lines);
        }
        public static void SaveRoundsToFile(this TournamentModel model, string matchupsPath, string matchupEntriesPath)
        {
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    matchup.SaveMatchupToFile();
                    
                }
            }
        }
        public static void SaveToTournamentsFile(this List<TournamentModel> models, string path)
        {
            List<string> lines=new List<string>();
            foreach (TournamentModel tm in models)
            {
                lines.Add($"{tm.Id}," +
                    $"{tm.TournamentName}," +
                    $"{ tm.EntryFee}," +
                    $"{ ConvertTeamsListToString(tm.EnteredTeams)}," +
                    $"{ ConvertPrizesListToString(tm.Prizes)}," +
                    $"{ ConvertRoundsListToString(tm.Rounds)}");
            }
            File.WriteAllLines(TextConnector.TournamentsPath, lines);

        }
        private static List<MatchupEntryModel> ConvertToMatchupEntryModel(this List<string> lines)
        {
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();
            foreach (string line in lines)
            {
                string[] columns = line.Split(',');
                MatchupEntryModel me = new MatchupEntryModel();
                me.Id = int.Parse(columns[0]);
                if (columns[1].Length==0)
                {
                    me.TeamCompeting = null;
                }
                else
                {
                    me.TeamCompeting = LookupTeamById(int.Parse(columns[1]));
                }
                me.Score =double.Parse(columns[2]);
                int ParentId = 0;
                if (int.TryParse(columns[3],out ParentId))
                {
                    me.ParentMatchup = LookupMatchupById(ParentId);
                }
                else
                {
                    me.ParentMatchup = null;
                }
                output.Add(me);
            }
            return output;
        }
        private static List<MatchupModel> ConvertToMatchupModel(this List<string> lines)
        {
            List<MatchupModel> output = new List<MatchupModel>();
            foreach (string line in lines)
            {
                string[] columns = line.Split(',');
                MatchupModel matchupModel = new MatchupModel();
                matchupModel.Id = int.Parse(columns[0]);
                matchupModel.Entries = ConvertStringToMatchupEntryModel(columns[1]);
                if (columns[2].Length==0)
                {
                    matchupModel.Winner = null;
                }
                else
                {
                    matchupModel.Winner = LookupTeamById(int.Parse(columns[2]));
                }
                
                matchupModel.MatchupRound = int.Parse(columns[3]);
                output.Add(matchupModel);
            }
            return output;
        }
        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModel(string input)
        {
            string[] ids=input.Split('|');
            List<MatchupEntryModel> output=new List<MatchupEntryModel>();
            List<string> entries = TextConnector.MatChupEntriesPath.LoadFile();
            List<string> matchingEntries = new List<string>();
            foreach(string id in ids)
            {
                foreach (string entry in entries)
                {
                    string[]cols=entry.Split(',');
                    if (cols[0]==id)
                    {
                        matchingEntries.Add(entry);
                    }
                }
            }
            output=matchingEntries.ConvertToMatchupEntryModel();
            return output;
        }
        private static TeamModel LookupTeamById(int id)
        {
            List<string> teams = TextConnector.TeamsPath.LoadFile();
            foreach(string team in teams)
            {
                string[]cols=team.Split(',');
                if (cols[0]==id.ToString())
                {
                    List<string> matchingTeams = new List<string>();
                    matchingTeams.Add(team);
                    return matchingTeams.ConvertToTeamModel().First();
                }
            }
            return null;
        }
        private static MatchupModel LookupMatchupById(int id)
        {
            List<string> matchups = TextConnector.MatchupsPath.LoadFile();
            foreach (string matchup in matchups)
            {
                string[] cols = matchup.Split(',');
                if (cols[0]==id.ToString())
                {
                    List<string> matchingMatchups = new List<string>();
                    matchingMatchups.Add(matchup);
                    return matchingMatchups.ConvertToMatchupModel().First();
                }
            }
            return null;
        }
        private static void SaveMatchupToFile(this MatchupModel matchup)
        {
            List<MatchupModel>matchups=TextConnector.MatchupsPath.LoadFile().ConvertToMatchupModel();
            int currentId = 1;
            
            if (matchups.Count() > 0)
            {
                currentId = matchups.OrderByDescending(x => x.Id).First().Id + 1;
            }
            matchup.Id = currentId;
            matchups.Add(matchup);
            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.SaveToMatchupEntriesFile();
            }
            List<string> lines = new List<string>();
            foreach (MatchupModel m in matchups)
            {
                string winner = "";
                if (m.Winner!=null)
                {
                    winner = m.Winner.Id.ToString();
                }
                 lines.Add($"{m.Id },{ ConvertMatchupEntryListToString(m.Entries) },{ winner },{ m.MatchupRound }");
            }
            File.WriteAllLines(TextConnector.MatchupsPath, lines);
        }
        private static void SaveToMatchupEntriesFile(this MatchupEntryModel entry)
        {
           List<MatchupEntryModel>entries=TextConnector.MatChupEntriesPath.LoadFile().ConvertToMatchupEntryModel();
            int currentId = 1;
            if (entries.Count() > 0)
            {
                currentId = entries.OrderByDescending(x => x.Id).First().Id + 1;
            }
            entry.Id = currentId;
            entries.Add(entry);
            List<string> lines = new List<string>();
            foreach (MatchupEntryModel e in entries)
            {
                string parent = "";
                if (e.ParentMatchup!=null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }
                string teamCompeting = "";
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{ e.Id },{ teamCompeting },{ e.Score },{ parent }");
            }
            File.WriteAllLines(TextConnector.MatChupEntriesPath, lines);
        }
        private static void UpdateMatchupEntriesFile(this MatchupEntryModel entry)
        {
            List<MatchupEntryModel> entries = TextConnector.MatChupEntriesPath.LoadFile().ConvertToMatchupEntryModel();
            MatchupEntryModel oldEntry = new MatchupEntryModel();
            foreach (MatchupEntryModel e in entries)
            {
                if (e.Id==entry.Id)
                {
                    oldEntry = e;
                }
            }
            entries.Remove(oldEntry);
            entries.Add(entry);
            List<string> lines = new List<string>();
            foreach (MatchupEntryModel e in entries)
            {
                string parent = "";
                if (e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }
                string teamCompeting = "";
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{ e.Id },{ teamCompeting },{ e.Score },{ parent }");
            }
            File.WriteAllLines(TextConnector.MatChupEntriesPath, lines);
        }
        private static string ConvertTeamsListToString(List<TeamModel> teams)
        {
            string output = "";
            if (teams.Count == 0)
            {
                return "";
            }
            foreach (TeamModel team in teams)
            {
                output += $"{team.Id}|";
            }
            output = output.Substring(0, output.Length - 1);
            return output;
        }
        private static string ConvertMatchupEntryListToString(List<MatchupEntryModel> entries)
        {
            string output = "";
            if (entries.Count == 0)
            {
                return "";
            }
            foreach (MatchupEntryModel e in entries)
            {
                output += $"{ e.Id }|";
            }
            output = output.Substring(0, output.Length - 1);
            return output;
        }
        private static string ConvertPrizesListToString(List<PrizeModel> prizes)
        {
            string output = "";
            if (prizes.Count == 0)
            {
                return "";
            }
            foreach (PrizeModel priz in prizes)
            {
                output += $"{priz.Id}|";
            }
            output = output.Substring(0, output.Length - 1);
            return output;
        }
        private static string ConvertRoundsListToString(List<List<MatchupModel>> rounds)
        {
            string output = "";
            if (rounds.Count == 0)
            {
                return "";
            }
            foreach (List<MatchupModel> round in rounds)
            {
                output += $"{ConvertMatchupsListToString(round)}|";
            }
            output = output.Substring(0, output.Length - 1);
            return output;
        }
        private static string ConvertMatchupsListToString(List<MatchupModel> matchups)
        {
            string output = "";
            if (matchups.Count == 0)
            {
                return "";
            }
            foreach (MatchupModel matchup in matchups)
            {
                output += $"{matchup.Id}^";
            }
            output = output.Substring(0, output.Length - 1);
            return output;
        }
        private static string ConvertPersonListToString(List<PersonModel> persons)
        {
            string output ="";
            if (persons.Count==0)
            {
                return "";
            }
            foreach (PersonModel person in persons)
            {
                output += $"{person.Id}|";
            }
            output=output.Substring(0, output.Length - 1);
            return output;
        }
    }
}
