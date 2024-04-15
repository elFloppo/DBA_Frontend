using System.Text.RegularExpressions;

namespace DBA_Frontend.Validators
{
    public static class Validator
    {
        public static bool IsSystemPathValid(string path)
        {
            return Regex.IsMatch(path, @"^([a-zA-Z]:\\)([-\u4e00-\u9fa5\w\s.()~!@#$%^&()\[\]{}+=]+\\)*$");
        }
    }
}
