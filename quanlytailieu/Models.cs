namespace StudyDocs
{
    public class SubjectItem
    {
        public int SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }

    public class DocumentItem
    {
        public int DocumentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? SubjectId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool Status { get; set; }
    }
}