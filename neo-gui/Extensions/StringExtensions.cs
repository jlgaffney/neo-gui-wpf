namespace Neo.Extensions
{
    public static class StringExtensions
    {
        public static string[] ToLines(this string source)
        {
            if (string.IsNullOrEmpty(source)) return new string[0];

            var lines = source.Split('\n');

            // Remove \r character from end of line if present
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line[line.Length - 1] == '\r')
                {
                    line = line.Substring(0, line.Length - 1);
                }

                lines[i] = line;
            }

            return lines;
        }
    }
}