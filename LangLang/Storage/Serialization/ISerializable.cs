namespace LangLang.Storage.Serialization
{
    public interface ISerializable
    {
        string ToJSON();

        void FromJSON(string value);
    }
}
