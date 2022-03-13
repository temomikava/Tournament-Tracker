using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;
using TrackerLibrary.DataAccess.TextHelpers;
namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {

        public static string PrizesPath { get; } = @"C:\data\TournamentTracker\PrizeModels.csv";
        public static string PersonsPath { get; } = @"C:\data\TournamentTracker\PersonModels.csv";
        public static string TeamsPath { get; } = @"C:\data\TournamentTracker\TeamModels.csv";
        
        public PersonModel CreatePerson(PersonModel model)
        {
            List<PersonModel> persons = PersonsPath.LoadFile().ConvertToPersonModels();
            int currentId = 1;
            if (persons.Count() > 0)
            {
                currentId = persons.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;
            persons.Add(model);
            persons.SaveToPersonFile(PersonsPath);
            return model;
        }

        public PrizeModel CreatePrize(PrizeModel model)
        {
            //Load the text file and convert the text to List<PrizeModel>
            List<PrizeModel> Prizes=PrizesPath.LoadFile().ConvertToPrizeModels();
            //Find the max ID
            int currentId = 1;
            if (Prizes.Count()>0)
            {
                currentId= Prizes.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id=currentId;
            //Add the new record with the new ID(ID=max+1)
            Prizes.Add(model);
            //Convert the prizes to list<string>
            //Save the list<string> to the text file
            Prizes.SaveToPrizeFile(PrizesPath);

            return model;
        }

        public TeamModel CreateTeam(TeamModel model)
        {
            List<TeamModel> teams=TeamsPath.LoadFile().ConvertToTeamModel();
            int currentId = 1;
            if (teams.Count() > 0)
            {
              currentId = teams.OrderByDescending(x => x.Id).First().Id + 1;
            }
            
            model.Id = currentId;
            teams.Add(model);
            teams.SaveToTeamFile(PersonsPath);
            return model;

        }

        public List<PersonModel> GetPerson_All()
        {
            return PersonsPath.LoadFile().ConvertToPersonModels();
        }

        public List<TeamModel> GetTeam_All()
        {
           return  TeamsPath.LoadFile().ConvertToTeamModel();
        }
    }
}
