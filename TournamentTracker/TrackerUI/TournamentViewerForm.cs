using System.ComponentModel;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class TournamentViewerForm : Form
    {
        private TournamentModel tournament;
        BindingList<int>rounds=new BindingList<int>();
        BindingList<MatchupModel> selectedMatchups = new BindingList<MatchupModel>();              
        public TournamentViewerForm(TournamentModel tournamentModel)
        {
            InitializeComponent();
            tournament = tournamentModel;
            InitializeLists();
            LoadFormData();
            LoadRounds();
        }
        private void LoadFormData()
        {
            tournamentName.Text = tournament.TournamentName;
        }
        private void InitializeLists()
        {
            roundDropDown.DataSource = rounds;
            matchupListBox.DataSource = selectedMatchups;
            matchupListBox.DisplayMember = "DisplayName";
        }      
        private void LoadRounds()
        {
            rounds.Clear();
            rounds.Add(1);
            int currRound = 1;
            foreach (List<MatchupModel> matchups in tournament.Rounds)
            {
                if (matchups.First().MatchupRound>currRound)
                {
                    currRound= matchups.First().MatchupRound;
                    rounds.Add(currRound);
                }
            }
            LoadMatchups(1);
        }

        private void roundDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }
        private void LoadMatchups(int round)
        {
            foreach (List<MatchupModel> matchups in tournament.Rounds)
            {
                if (matchups.First().MatchupRound == round)
                {
                    selectedMatchups.Clear();
                    foreach (MatchupModel m in matchups)
                    {
                        if (m.Winner == null || !unplayedOnlyCheckBox.Checked)
                        {
                            selectedMatchups.Add(m);
                        }
                    }
                }
            }
            if (selectedMatchups.Count > 0)
            {
                LoadMatchup(selectedMatchups.First());
            }
            DisplayMatchupInfo();
        }
        private void DisplayMatchupInfo()
        {
            bool isVisable=selectedMatchups.Count > 0;

            teamOneName.Visible=isVisable;
            teamOneScoreLabel.Visible=isVisable;
            teamOneScoreValue.Visible=isVisable;
            TeamTwoName.Visible=isVisable;
            teamTwoScoreLabel.Visible=isVisable;
            teamTwoScoreValue.Visible=isVisable;
            scoreButton.Visible=isVisable;
            versusLabel.Visible=isVisable;
        }
        private void LoadMatchup(MatchupModel m)
        {
            if (m!=null)
            {
                for (int i = 0; i < m.Entries.Count; i++)
                {
                    if (i == 0)
                    {
                        if (m.Entries[0].TeamCompeting != null)
                        {
                            teamOneName.Text = m.Entries[0].TeamCompeting.TeamName;
                            teamOneScoreValue.Text = m.Entries[0].Score.ToString();
                            TeamTwoName.Text = "<bye>";
                            teamTwoScoreValue.Text = "0";
                        }
                        else
                        {
                            teamOneName.Text = "not yet set";
                            teamOneScoreValue.Text = "";                          
                        }
                    }
                    if (i == 1)
                    {
                        if (m.Entries[1].TeamCompeting != null)
                        {
                            TeamTwoName.Text = m.Entries[1].TeamCompeting.TeamName;
                            teamTwoScoreValue.Text = m.Entries[1].Score.ToString();
                        }
                        else
                        {
                            TeamTwoName.Text = "not yet set";
                            teamTwoScoreValue.Text = "";
                        }
                    }
                }
            }
        }
        private void matchupListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchup((MatchupModel)matchupListBox.SelectedItem);
        }
        private void unplayedOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }
        private string IsValidData()
        {
            bool scoreOneValid = double.TryParse(teamOneScoreValue.Text, out double teamOneScore);
            bool scoreTwoValid = double.TryParse(teamTwoScoreValue.Text, out double teamTwoScore);

            string output = !scoreOneValid ? "Score one value is not a valid number" : 
                !scoreTwoValid ? "Score Two value is not a valid number" :
                (teamOneScore == 0 && teamTwoScore == 0) ? "you did not enter a score for either team" :
                teamOneScore == teamTwoScore ? "we don't allow ties in this application" : "";
            return output;
        }
        private void scoreButton_Click(object sender, EventArgs e)
        {
            string errorMessage = IsValidData();
            if (errorMessage.Length>0)
            {
                MessageBox.Show(errorMessage);
                return;
            }
            MatchupModel m = (MatchupModel)matchupListBox.SelectedItem;           
            for (int i = 0; i < m.Entries.Count; i++)
            {
                if (i == 0)
                {
                    if (m.Entries[0].TeamCompeting != null)
                    {
                        m.Entries[0].Score = double.Parse(teamOneScoreValue.Text); ;
                    }
                }
                if (i == 1)
                {
                    if (m.Entries[0].TeamCompeting != null)
                    {
                        m.Entries[1].Score = double.Parse(teamTwoScoreValue.Text);
                    }
                }
            }
            
            TournamentLogic.UpdateTournamentResults(tournament);
            GlobalConfig.Connection.UpdateMatchup(m);
        }
    }
}