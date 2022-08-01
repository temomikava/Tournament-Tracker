using TrackerLibrary.DataAccess.TextHelpers;
using TrackerLibrary.Models;
namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {
        internal static string PrizesPath { get; } = @"C:\data\TournamentTracker\PrizeModels.csv";
        internal static string PersonsPath { get; } = @"C:\data\TournamentTracker\PersonModels.csv";
        internal static string TeamsPath { get; } = @"C:\data\TournamentTracker\TeamModels.csv";
        internal static string TournamentsPath { get; } = @"C:\data\TournamentTracker\TournamentModels.csv";
        internal static string MatchupsPath { get; } = @"C:\data\TournamentTracker\MatchupModels.csv";
        internal static string MatChupEntriesPath { get; } = @"C:\data\TournamentTracker\MatchupEntryModels.csv";


        public void CreatePerson(PersonModel model)
        {
            List<PersonModel> persons = PersonsPath.LoadFile().ConvertToPersonModels();
            int currentId = 1;
            if (persons.Count() > 0)
            {
                currentId = persons.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;
            persons.Add(model);
            persons.SaveToPersonsFile(PersonsPath);

        }

        public void CreatePrize(PrizeModel model)
        {
            //Load the text file and convert the text to List<PrizeModel>
            List<PrizeModel> Prizes = PrizesPath.LoadFile().ConvertToPrizeModels();
            //Find the max ID
            int currentId = 1;
            if (Prizes.Count() > 0)
            {
                currentId = Prizes.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;
            //Add the new record with the new ID(ID=max+1)
            Prizes.Add(model);
            //Convert the prizes to list<string>
            //Save the list<string> to the text file
            Prizes.SaveToPrizesFile(PrizesPath);


        }

        public void CreateTeam(TeamModel model)
        {
            List<TeamModel> teams = TeamsPath.LoadFile().ConvertToTeamModel();
            int currentId = 1;
            if (teams.Count() > 0)
            {
                currentId = teams.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;
            teams.Add(model);
            teams.SaveToTeamsFile(PersonsPath);


        }

        public void CreateTournament(TournamentModel model)
        {
            List<TournamentModel> Tournaments = TournamentsPath.LoadFile().ConvertToTournamentModel();
            int currentId = 1;
            if (Tournaments.Count() > 0)
            {
                currentId = Tournaments.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;
            model.SaveRoundsToFile(MatchupsPath, MatChupEntriesPath);
            Tournaments.Add(model);
            Tournaments.SaveToTournamentsFile(TournamentsPath);
            TournamentLogic.UpdateTournamentResults(model);

        }

        public List<PersonModel> GetPerson_All()
        {
            return PersonsPath.LoadFile().ConvertToPersonModels();
        }

        public List<TeamModel> GetTeam_All()
        {
            return TeamsPath.LoadFile().ConvertToTeamModel();
        }

        public List<TournamentModel> GetTournament_All()
        {
            return TournamentsPath.LoadFile().ConvertToTournamentModel();
        }

        public void UpdateMatchup(MatchupModel model)
        {
            model.UpdateMatchupToFile();
        }
    }
}
