namespace ModernWPF.Client.Features.Controls
{
    public static class InitiatorInfo
    {
        public static string Generate(string callerMember = null, string callerFile = null, int callerLine = 0)
        {
            var initiatorInfo = callerMember ?? "(Unknown)";

            if (!string.IsNullOrEmpty(callerFile))
            {
                initiatorInfo += string.Format("({0}:{1})", callerFile, callerLine);
            }

            return initiatorInfo;
        }
    }
}