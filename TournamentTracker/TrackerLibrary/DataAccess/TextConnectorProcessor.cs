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

        public static void SaveToPrizeFile(this List<PrizeModel> models, string path)
        {
            List<string>lines=new List<string>();
            foreach (PrizeModel p in models)
            {
                lines.Add($"{p.Id },{ p.PlaceNumber },{ p.PlaceName },{ p.PrizeAmount },{ p.PrizePercentage}");
            }
            File.WriteAllLines(TextConnector.PrizesPath, lines);
        }
        public static void SaveToPersonFile(this List<PersonModel> models, string path)
        {
            List<string> lines = new List<string>();
            foreach (PersonModel p in models)
            {
                lines.Add($"{p.Id },{ p.FirstName },{ p.LastName },{ p.EmailAddress },{ p.CellphoneNumber}");
            }
            File.WriteAllLines(TextConnector.PersonsPath, lines);
        }
        public static void SaveToTeamFile(this List<TeamModel> models, string path)
        {
            List<string> lines = new List<string>();
            foreach(TeamModel t in models)
            {
                
                lines.Add($"{ t.Id },{ t.TeamName },{ConvertPersonListToString(t.TeamMembers)}");              
            }
            File.WriteAllLines(TextConnector.TeamsPath, lines);
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
