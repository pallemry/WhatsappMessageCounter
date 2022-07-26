namespace WhatsappMessageCounterLibrary.Data_Classes
{
    public class ProgressionInformation
    {
        int precentage = 0;
        public int Precentage
        {
            get
            {
                return precentage;
            }

            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(value));
                precentage = value;
            }
        }
        public string Message { get; set; }
    }
}