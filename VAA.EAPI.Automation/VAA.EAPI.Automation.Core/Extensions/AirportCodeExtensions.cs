public class AirportCodeMapper
{
    private readonly Dictionary<string, string> _codeMap;

    public AirportCodeMapper()
    {
        _codeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"LON", "LHR"},
            // Add more mappings here
        };
    }

    public string MapCode(string code)
    {
        if (_codeMap.TryGetValue(code, out string mappedCode))
        {
            return mappedCode;
        }
        return code; // Return original code if no mapping exists
    }

    //Method to add new mappings dynamically
    public void AddMapping(string fromCode, string toCode)
    {
        _codeMap[fromCode] = toCode;
    }
}