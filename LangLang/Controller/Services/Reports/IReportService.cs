namespace LangLang.Controller.Services.Reports
{
    internal interface IReportService
    {
        string Key { get; }
        void GenerateReport();
    }
}
