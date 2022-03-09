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

namespace TrackerUI
{
    public partial class CreatePrizeForm : Form
    {
        public CreatePrizeForm()
        {
            InitializeComponent();
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                PrizeModel model = new PrizeModel();
                model.PlaceNumber = int.Parse(placeNumberValue.Text);
                model.PlaceName = placeNameValue.Text;
                model.PrizeAmount=decimal.Parse(prizeAmountValue.Text);
                model.PrizePercentage=double.Parse(prizePercentageValue.Text);

                foreach(IDataConnection db in GlobalConfig.Connections)
                {
                    db.CreatePrize(model);
                }
                MessageBox.Show("Done");
                placeNumberValue.Text = "";
                placeNameValue.Text = "";
                prizeAmountValue.Text = "0";
                prizePercentageValue.Text = "0";
            }
            else
            {
                MessageBox.Show("This form has invalid information. Please, check it and try again");
            }
        }
        private bool ValidateForm()
        {
            bool output = true;
            int placeNumber = 0;
            bool placeNumberValidation=int.TryParse(placeNumberValue.Text, out placeNumber);
            if (!placeNumberValidation)
            {
                output = false;
            }
            if (placeNumber<1)
            {
                output=false;
            }
            if (placeNumberValue.Text.Length==0)
            {
                output = false;
            }
            decimal prizeAmount = 0;
            double prizePercentage = 0;
            bool prizeAmountValidation=decimal.TryParse(prizeAmountValue.Text, out prizeAmount);
            bool prizePercentageValidation=double.TryParse(prizePercentageValue.Text, out prizePercentage);
            if (!prizeAmountValidation || !prizePercentageValidation)
            {
                output = false;
            }
            if (prizeAmount<=0 && prizePercentage<=0)
            {
                output = false;
            }
            if (prizePercentage<0 || prizePercentage>100)
            {
                output = false;
            }
            return output;
        }
    }
}
