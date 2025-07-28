namespace api.Models
{
    public class ClassroomEnrollment
    {
        public int Id { get; set; }
        public int ClassroomId { get; set; }
        public int StudentId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public Classroom? Classroom { get; set; }
        public Student? Student { get; set; }
    }
}
