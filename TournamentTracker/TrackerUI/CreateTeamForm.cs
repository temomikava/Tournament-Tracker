using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    
    public partial class CreateTeamForm : Form
    {
        List<PersonModel> availableTeamMembers = GlobalConfig.Connection.GetPerson_All();
        List<PersonModel>selectedTeamMembers=new List<PersonModel>();
        ITeamRequestor callingForm;
        public CreateTeamForm(ITeamRequestor caller )
        {
            InitializeComponent(); 
            callingForm = caller;
            InitializeLists();
        }
        private void SampleData()
        {
            availableTeamMembers.Add(new PersonModel() { FirstName = "temo", LastName="mika" });
            availableTeamMembers.Add(new PersonModel() { FirstName = "gio", LastName = "kika" });
            selectedTeamMembers.Add(new PersonModel() { FirstName = "lore", LastName = "pasa" });
            selectedTeamMembers.Add(new PersonModel() { FirstName = "mike", LastName = "lori" });

        }
        private void InitializeLists()
        {
            selectTeamMemberDropDown.DataSource = null;
            selectTeamMemberDropDown.DataSource = availableTeamMembers;
            selectTeamMemberDropDown.DisplayMember = "FullName";

            teamMembersListBox.DataSource = null;
            teamMembersListBox.DataSource = selectedTeamMembers;
            teamMembersListBox.DisplayMember = "FullName";
        }

        private void createMemberButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                PersonModel p = new PersonModel();
                p.FirstName = firstNameValue.Text;
                p.LastName = lastNameValue.Text;
                p.EmailAddress = emailValue.Text;
                p.CellphoneNumber = cellPhoneValue.Text;

                p=GlobalConfig.Connection.CreatePerson(p);
                selectedTeamMembers.Add(p);

                firstNameValue.Text = "";
                lastNameValue.Text = "";
                emailValue.Text = "";
                cellPhoneValue.Text = "";


                MessageBox.Show("Done!");
            }
            else
            {
                MessageBox.Show("This form has invalid information. Please, check it and try again");
            }
        }
        private bool ValidateForm()
        {
            //TODO -Add validation to the form
            if (firstNameValue.Text.Length == 0 ||
                lastNameValue.Text.Length == 0 ||
                emailValue.Text.Length == 0 ||
                cellPhoneValue.Text.Length == 0)
            {
                return false;
            }
            return true;
        }

        private void addMemberButton_Click(object sender, EventArgs e)
        {
            PersonModel p=(PersonModel)selectTeamMemberDropDown.SelectedItem;
            if (p != null)
            {
                availableTeamMembers.Remove(p);
                selectedTeamMembers.Add(p);
                InitializeLists();
            }
           
        }

        private void removeSelectedmemberButton_Click(object sender, EventArgs e)
        {
            PersonModel p = (PersonModel)teamMembersListBox.SelectedItem;
            if (p!=null)
            {
                selectedTeamMembers.Remove(p);
                availableTeamMembers.Add(p);
                InitializeLists();
            }
           
        }

        private void createTeamButton_Click(object sender, EventArgs e)
        {
            if (!(teamNameValue.Text.Length==0 || selectedTeamMembers.Count()<2))
            {
                TeamModel t = new TeamModel();
                t.TeamName = teamNameValue.Text;
                t.TeamMembers = selectedTeamMembers;
                GlobalConfig.Connection.CreateTeam(t);
                callingForm.TeamComplete(t);
                InitializeLists();
                MessageBox.Show($"Team \"{t.TeamName}\" created successfully!");
                Close();

            }
            else if(teamNameValue.Text.Length == 0)
            {
                MessageBox.Show("Team Name value can not be empty");              
            }
            else
            {
                MessageBox.Show("Team members count can not be less than 2");

            }
           
        }
    }
    
}
