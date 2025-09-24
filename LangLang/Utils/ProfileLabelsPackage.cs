using System.Windows.Controls;

namespace LangLang.Utils
{
    public class ProfileLabelsPackage
    {
        public Label NameLabel { get; }
        public Label SurnameLabel { get; }
        public Label GenderLabel { get; }
        public Label BirthDateLabel { get; }
        public Label PhoneLabel { get; }
        public Label EmailLabel { get; }
        public Label EducationLabel { get; }
        public Label CurrentCourseLabel { get; }
        public Label PenaltyPointsLabel { get; }
        public Label JoinedDateLabel { get; }

        public ProfileLabelsPackage(Label nameLabel, Label surnameLabel, Label genderLabel, Label birthDateLabel, Label phoneLabel, Label emailLabel, Label educationLabel, Label currentCourseLabel, Label penaltyPointsLabel, Label joinedDateLabel)
        {
            NameLabel = nameLabel;
            SurnameLabel = surnameLabel;
            GenderLabel = genderLabel;
            BirthDateLabel = birthDateLabel;
            PhoneLabel = phoneLabel;
            EmailLabel = emailLabel;
            EducationLabel = educationLabel;
            CurrentCourseLabel = currentCourseLabel;
            PenaltyPointsLabel = penaltyPointsLabel;
            JoinedDateLabel = joinedDateLabel;
        }
    }
}
