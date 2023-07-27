using System.ComponentModel;

namespace ArtemisManagerAction
{
    public interface IINISetting
    {
        bool BoolValue { get; set; }
        List<string> CommentLines { get; set; }
        double DoubleValue { get; set; }
        string FileValue { get; set; }
        int IntValue { get; set; }
        string SettingName { get; set; }
        bool UsingDefault { get; set; }

        event PropertyChangedEventHandler? PropertyChanged;

        bool GetBoolValue();
        double GetDoubleValue();
        int GetIntValue();
        string[] GetOutLines();
        void ProcessLine(string line);
        void SetValue(bool value);
        void SetValue(double value);
        void SetValue(int value);
        void SetValue(string value);
        string ToString();
    }
}