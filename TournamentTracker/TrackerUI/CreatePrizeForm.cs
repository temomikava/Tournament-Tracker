using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class CreatePrizeForm : Form
    {
        IPrizeRequestor CallingForm;
        public CreatePrizeForm(IPrizeRequestor caller)
        {
            InitializeComponent();
            CallingForm = caller;
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                PrizeModel model = new PrizeModel();
                model.PlaceNumber = int.Parse(placeNumberValue.Text);
                model.PlaceName = placeNameValue.Text;
                model.PrizeAmount = decimal.Parse(prizeAmountValue.Text);
                model.PrizePercentage = double.Parse(prizePercentageValue.Text);

                GlobalConfig.Connection.CreatePrize(model);

                CallingForm.PrizeComplete(model);

                MessageBox.Show("Done");
                Close();
                //placeNumberValue.Text = "";
                //placeNameValue.Text = "";
                //prizeAmountValue.Text = "0";
                //prizePercentageValue.Text = "0";
            }
            else
            {
                MessageBox.Show("This form has invalid information. Please, check it and try again");
            }
        }
        private bool ValidateForm()
        {
            int placeNumber = 0;
            bool placeNumberValidation = int.TryParse(placeNumberValue.Text, out placeNumber);
            if (!placeNumberValidation ||
                placeNumber < 1 ||
                placeNumberValue.Text.Length == 0)

            {
                return false;
            }

            decimal prizeAmount = 0;
            double prizePercentage = 0;
            bool prizeAmountValidation = decimal.TryParse(prizeAmountValue.Text, out prizeAmount);
            bool prizePercentageValidation = double.TryParse(prizePercentageValue.Text, out prizePercentage);
            if (!prizeAmountValidation || !prizePercentageValidation)
            {
                return false;
            }
            if (prizeAmount < 0 || (prizeAmount == 0 && prizePercentage != 0))
            {
                return false;
            }
            if (prizePercentage < 0 || prizePercentage > 100)
            {
                return false;

            }
            return true;
        }
    }
}
