using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Configuration;



namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        readonly string connectionString = "Server=DESKTOP-0LBLIAV;Database=Tournaments;Trusted_Connection=True;";

        public PersonModel CreatePerson(PersonModel model)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var p = new DynamicParameters();
                p.Add("@FirstName", model.FirstName);
                p.Add("@LastName", model.LastName);
                p.Add("@EmailAddress", model.EmailAddress);
                p.Add("@CellphoneNumber", model.CellphoneNumber);
                p.Add("@ID", 0, DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("spPeople_Insert", p, commandType: CommandType.StoredProcedure);
                model.Id = p.Get<int>("@ID");
                return model;
            }
        }

        /// <summary>
        /// Saves the new prize to the database.
        /// </summary>
        /// <param name="model">The prize information.</param>
        /// <returns>Prize information including the unique identifier</returns>

        public PrizeModel CreatePrize(PrizeModel model)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var p = new DynamicParameters();
                p.Add("@PlaceNumber", model.PlaceNumber);
                p.Add("@PlaceName", model.PlaceName);
                p.Add("@PrizeAmount", model.PrizeAmount);
                p.Add("@PrizePercentage", model.PrizePercentage);
                p.Add("@ID", 0, DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("spPrizes_Insert", p, commandType: CommandType.StoredProcedure);
                model.Id = p.Get<int>("@ID");
                return model;
            }
            
        }

        public TeamModel CreateTeam(TeamModel model)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var p = new DynamicParameters();
                p.Add("@TeamName", model.TeamName);
                p.Add("@ID", 0, DbType.Int32, direction: ParameterDirection.Output);
                connection.Execute("spTeams_Insert", p, commandType: CommandType.StoredProcedure);
                model.Id = p.Get<int>("@ID");
                foreach (PersonModel tm in model.TeamMembers)
                {
                    p = new DynamicParameters();
                    p.Add("@TeamID", model.Id);
                    p.Add("@PersonID", tm.Id);
                    connection.Execute("spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);
                }
                return model;
            }
        }

        public void CreateTournament(TournamentModel model)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                SaveTournaments(connection, model);
                SaveTournamentPrizes(connection, model);
                SaveTournamentEntries(connection, model);
                SaveTournamentRounds(connection, model);
            }
        }
        private void SaveTournaments(IDbConnection connection, TournamentModel model)
        {
            var p = new DynamicParameters();
            p.Add("@TournamentName", model.TournamentName);
            p.Add("@EntryFee", model.EntryFee);
            p.Add("@ID", 0, DbType.Int32, direction: ParameterDirection.Output);
            connection.Execute("dbo.spTournaments_insert", p, commandType: CommandType.StoredProcedure);
            model.Id = p.Get<int>("@ID");
        }
        private void SaveTournamentPrizes(IDbConnection connection, TournamentModel model)
        {
            foreach (PrizeModel pz in model.Prizes)
            {
                var p = new DynamicParameters();
                p.Add("@TournamentID", model.Id);
                p.Add("@PrizeID", pz.Id);
                p.Add("@ID", 0, DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
            }
            if (model.Prizes.Count==0)
            {
                var p = new DynamicParameters();
                p.Add("@TournamentID", model.Id);
                p.Add("@PrizeID", null);
                p.Add("@ID", 0, DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }
        private void SaveTournamentEntries(IDbConnection connection, TournamentModel model)
        {
            foreach (TeamModel tm in model.EnteredTeams)
            {
                var p = new DynamicParameters();
                p.Add("@TournamentID", model.Id);
                p.Add("@TeamID", tm.Id);
                p.Add("@ID", 0, DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }
        private void SaveTournamentRounds(IDbConnection connection, TournamentModel model)
        {
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    var p = new DynamicParameters();
                    p.Add("@TournamentID", model.Id);
                    p.Add("@MatchupRound", matchup.MatchupRound);
                    p.Add("@ID", 0, DbType.Int32, direction: ParameterDirection.Output);

                    connection.Execute("dbo.spMatchups_Insert", p, commandType: CommandType.StoredProcedure);
                    matchup.Id = p.Get<int>("@ID");
                    foreach (MatchupEntryModel entry in matchup.Entries)
                    {
                        p = new DynamicParameters();
                        
                        p.Add("@MatchupID", matchup.Id);
                        if (entry.ParentMatchup==null)
                        {
                            p.Add("@ParentMatchupID", null);
                        }
                        else
                        {
                           p.Add("@ParentMatchupID", entry.ParentMatchup.Id);
                        }
                        if (entry.TeamCompeting == null)
                        {
                            p.Add("@TeamCompeting", null);
                        }
                        else
                        {
                            p.Add("@TeamCompeting", entry.TeamCompeting.Id);
                        }
                        
                        p.Add("@ID", 0, DbType.Int32, direction: ParameterDirection.Output);

                        connection.Execute("dbo.spMatchupEntries_Insert", p, commandType: CommandType.StoredProcedure);
                    }
                }
            }
        }
        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;
            using(IDbConnection connection = new SqlConnection(connectionString))
            {
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }
            return output;
        }

        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                output = connection.Query<TeamModel>("dbo.spTeams_GetAll").ToList();
                foreach (TeamModel team in output)
                {
                    var p = new DynamicParameters();
                    p.Add("@TeamID", team.Id);
                    team.TeamMembers=connection.Query<PersonModel>("dbo.spTeammembers_GetByTeams",p, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            return output;
        }
    }
}
