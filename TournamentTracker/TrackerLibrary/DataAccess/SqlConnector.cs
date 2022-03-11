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
        //    @PlaceNumber int,
        //@PlaceName nvarchar(50),
        //@PrizeAmount money,
        //   @PrizePercentage float,
        //@ID int=0 output
        //TODO - Make the CreatePrize method actually save to the database.
        /// <summary>
        /// Saves the new prize to the database.
        /// </summary>
        /// <param name="model">The prize information.</param>
        /// <returns>Prize information including the unique identifier</returns>
        string connectionString = "Server=DESKTOP-0LBLIAV;Database=Tournaments;Trusted_Connection=True;";

        public PrizeModel CreatePrize(PrizeModel model)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                var p = new DynamicParameters();
                p.Add("@PlaceNumber", model.PlaceNumber);
                p.Add("@PlaceName", model.PlaceName);
                p.Add("@PrizeAmount", model.PrizeAmount);
                p.Add("@PrizePercentage", model.PrizePercentage);
                p.Add("ID", 0, DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("spPrizes_Insert", p, commandType: CommandType.StoredProcedure);
                model.Id = p.Get<int>("@ID");
                return model;
            }
            
        }
    }
}
