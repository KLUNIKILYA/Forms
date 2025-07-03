namespace Forms.Models.Forms
{
    public class SubmitFormRequest
    {
        public int FormId { get; set; }

        public Dictionary<int, object> Answers { get; set; } = new();
    }
}
