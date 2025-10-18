using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace StudyDocs
{
    public static class Db
    {
        private static readonly string _cs = ConfigurationManager.ConnectionStrings["Db"].ConnectionString;

        public static DataTable GetSubjects()
        {
            using (var con = new SqlConnection(_cs))
            using (var da = new SqlDataAdapter("SELECT SubjectId, Name FROM dbo.[Subject] ORDER BY Name", con))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public static DataTable GetDocuments(string keyword, int? subjectId, string type)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                SELECT d.DocumentId, d.Title, s.Name AS Subject, d.[Type], d.FilePath, d.Notes, d.LastOpened, d.[Status]
                FROM dbo.[Document] d
                LEFT JOIN dbo.[Subject] s ON d.SubjectId = s.SubjectId
                WHERE (@kw='' OR d.Title LIKE '%' + @kw + '%')
                  AND (@sid IS NULL OR d.SubjectId=@sid)
                  AND (@tp IS NULL OR d.[Type]=@tp)
                ORDER BY d.CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@kw", keyword ?? string.Empty);
                cmd.Parameters.AddWithValue("@sid", subjectId.HasValue ? (object)subjectId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@tp", string.IsNullOrEmpty(type) ? (object)DBNull.Value : type);

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static int InsertDocument(DocumentItem d)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.[Document](Title, SubjectId, [Type], FilePath, Notes, [Status])
                OUTPUT INSERTED.DocumentId
                VALUES(@t, @sid, @tp, @path, @notes, @st)
            ", con))
            {
                cmd.Parameters.AddWithValue("@t", d.Title);
                cmd.Parameters.AddWithValue("@sid", d.SubjectId.HasValue ? (object)d.SubjectId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@tp", d.Type);
                cmd.Parameters.AddWithValue("@path", string.IsNullOrWhiteSpace(d.FilePath) ? (object)DBNull.Value : d.FilePath);
                cmd.Parameters.AddWithValue("@notes", string.IsNullOrWhiteSpace(d.Notes) ? (object)DBNull.Value : d.Notes);
                cmd.Parameters.AddWithValue("@st", d.Status);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public static void UpdateDocument(DocumentItem d)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
                UPDATE dbo.[Document]
                SET Title=@t, SubjectId=@sid, [Type]=@tp, FilePath=@path, Notes=@notes, [Status]=@st
                WHERE DocumentId=@id
            ", con))
            {
                cmd.Parameters.AddWithValue("@t", d.Title);
                cmd.Parameters.AddWithValue("@sid", d.SubjectId.HasValue ? (object)d.SubjectId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@tp", d.Type);
                cmd.Parameters.AddWithValue("@path", string.IsNullOrWhiteSpace(d.FilePath) ? (object)DBNull.Value : d.FilePath);
                cmd.Parameters.AddWithValue("@notes", string.IsNullOrWhiteSpace(d.Notes) ? (object)DBNull.Value : d.Notes);
                cmd.Parameters.AddWithValue("@st", d.Status);
                cmd.Parameters.AddWithValue("@id", d.DocumentId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteDocument(int id)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("DELETE FROM dbo.[Document] WHERE DocumentId=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateLastOpened(int id)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("UPDATE dbo.[Document] SET LastOpened=GETDATE() WHERE DocumentId=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void InsertSubject(string name)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("INSERT INTO dbo.[Subject]([Name]) VALUES(@n)", con))
            {
                cmd.Parameters.AddWithValue("@n", name);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateSubject(int id, string newName)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("UPDATE dbo.[Subject] SET [Name]=@n WHERE SubjectId=@id", con))
            {
                cmd.Parameters.AddWithValue("@n", newName);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteSubject(int id)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("DELETE FROM dbo.[Subject] WHERE SubjectId=@id", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

    }
}