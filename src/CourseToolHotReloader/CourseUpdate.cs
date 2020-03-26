﻿﻿namespace CourseToolHotReloader
{
    
    public interface ICourseUpdate
    {
        string Name { get; set; }
        string RelativePath { get; set; }
    }
    
    public class CourseUpdate : ICourseUpdate
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
    }
}