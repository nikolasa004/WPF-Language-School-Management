using LangLang.Model.Enums;

namespace LangLang.Utils
{
    public class CoursesSearchFilter
    {
        public Language? Language { get; set; }
        public LanguageLevel? Level { get; set; }
        public bool? IsLive { get; set; }
        public int Weeks { get; set; }

        public CoursesSearchFilter(Language? language, LanguageLevel? level, bool? isLive, int weeks)
        {
            Language = language;
            Level = level;
            IsLive = isLive;
            Weeks = weeks;
        }
    }
}
