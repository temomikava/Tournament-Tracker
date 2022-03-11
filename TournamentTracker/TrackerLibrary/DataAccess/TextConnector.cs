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
        internal static string PrizesPath = @"C:\data\TournamentTracker\PrizeModels.csv";
        //TODO - Wire up the CreatePrize method for text files.
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
    }
}
