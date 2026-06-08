namespace EmkCda.Models.VIMIS
{
    public class RecordTarget
    {
        public PatientRole PatientRole { get; set; }

        public RecordTarget()
        {
            PatientRole = new PatientRole();
        }
    }
}
