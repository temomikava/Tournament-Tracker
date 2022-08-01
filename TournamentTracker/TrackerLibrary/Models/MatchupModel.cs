namespace TrackerLibrary.Models
{
    /// <summary>
    /// Represents one match in the tournament.
    /// </summary>
    public class MatchupModel
    {
        /// <summary>
        /// The unique identifier for the matchup
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The set of team that were involved in the match.
        /// </summary>
        public List<MatchupEntryModel> Entries { get; set; } = new List<MatchupEntryModel>();
        /// <summary>
        /// The id from the database that will be used to identify the winner
        /// </summary>
        public int WinnerId { get; set; }
        /// <summary>
        /// The winner of the match.
        /// </summary>
        public TeamModel Winner { get; set; }
        /// <summary>
        /// Which round this match is a part of.
        /// </summary>
        public int MatchupRound { get; set; }
        public string DisplayName
        {
            get
            {
                string output = "";
                foreach (MatchupEntryModel entry in Entries)
                {
                    if (entry.TeamCompeting != null)
                    {
                        if (output.Length == 0)
                        {
                            output = entry.TeamCompeting.TeamName;
                        }
                        else
                        {
                            output += $" vs. {entry.TeamCompeting.TeamName}";
                        }
                    }
                    else
                    {
                        output = "Matchup not yet determened";
                        break;
                    }
                }
                return output;
            }
        }
    }
}
