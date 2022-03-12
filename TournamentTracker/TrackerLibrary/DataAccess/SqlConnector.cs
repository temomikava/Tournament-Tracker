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
        string connectionString = "Server=DESKTOP-0LBLIAV;Database=Tournaments;Trusted_Connection=True;";

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

        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;
            using(IDbConnection connection = new SqlConnection(connectionString))
            {
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }
            return output;
        }
    }
}
