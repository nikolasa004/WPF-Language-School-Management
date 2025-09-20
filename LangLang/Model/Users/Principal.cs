using LangLang.Model.Enums;


namespace LangLang.Model.Users

{
    public class Principal : User
    {
        public Principal()
            : base("Principal", "Principal", Gender.Male, "1970-01-01", "000000000", "principal@gmail.com", "123", EducationLevel.Master, "2000-01-01")
        {
        }
    }
}
