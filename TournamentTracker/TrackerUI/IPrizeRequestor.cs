using TrackerLibrary.Models;

namespace TrackerUI
{
    public interface IPrizeRequestor
    {
        void PrizeComplete(PrizeModel model);
    }
}
