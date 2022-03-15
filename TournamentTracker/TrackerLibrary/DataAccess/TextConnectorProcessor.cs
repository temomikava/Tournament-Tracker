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
    //Convert the text to List<PrizeModel>
    //Find the max ID
    //Add the new record with the new ID(ID=max+1)
    //Convert the prizes to list<string>
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
                    tm.Prizes.Add(Prizes.Where(x => x.Id == int.Parse(id)).First());

                }
                //TODO - capture Rounds information
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
        public static void SaveToTournamentsFile(this List<TournamentModel> models, string path)
        {
            List<string> lines=new List<string>();
            foreach (TournamentModel tm in models)
            {
                lines.Add($@"{tm.Id},
                     {tm.TournamentName},
                     {tm.EntryFee},
                     {ConvertTeamsListToString(tm.EnteredTeams)},
                     {ConvertPrizesListToString(tm.Prizes)},
                     {ConvertRoundsListToString(tm.Rounds)}");
            }
            File.WriteAllLines(TextConnector.TournamentsPath, lines);

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
