using System.IO;
using System.Text.RegularExpressions;

namespace ToreAurstadIT.Logger
{

    /// <summary>
    /// A rolling logger that increments a logger when the size has filled up enough space defined by the 'MaxTraceFileSize' attribute.
    /// The naming convention increments the logging file name to the next available num
    /// </summary>
    public class RollingNumXmlWriterTraceListener : RollingXmlWriterTraceListenerbase
    {

        /// <summary>
        /// This expression is used to find the number of a trace file in its file name by searching for an underscore (_), a
        /// numeric expression with any repetitions and a dot (that marks the beginning of the file extension). The named 
        /// capture group named by the constant &quot;LogFileNumberCaptureName&quot; will contain the number.
        /// </summary>
        /// <remarks>Example file name: SomeAcmeTraceLogs_0001.svclog followed by SomeAcmeTraceLogs_0002.svclog and so on</remarks>
        private readonly Regex _logfileSuffixExpression = new Regex(@"_(?<" + LogFileNumberCaptureName + @">\d*)\.", RegexOptions.Compiled);

        /// <summary>
        /// This is the named Regex capture group to find the numeric suffix of a trace file
        /// </summary>
        private static readonly string LogFileNumberCaptureName = "LogFileNumber";

        public RollingNumXmlWriterTraceListener(string filename) : base(filename)
        {
        }

        public RollingNumXmlWriterTraceListener(string filename, string name) : base(filename, name)
        {
        }

        protected override void IncrementTraceFileNameSuffix()
        {
            if (string.IsNullOrWhiteSpace(CurrentFileNameSuffix))
            {
                CurrentFileNameSuffix = "1";
            }
            else
            {
                if (int.TryParse(CurrentFileNameSuffix, out int currentFileNameSuffixNum))
                {
                    CurrentFileNameSuffix = (++currentFileNameSuffixNum).ToString();
                }
            }
        }

        protected override string GetTraceFileNameSuffix()
        {
            string directoryName = Path.GetDirectoryName(BasicTraceFileName);
            string basicTraceFileNameWithoutExtension = Path.GetFileNameWithoutExtension(BasicTraceFileName);
            if (directoryName != null)
            {
                string[] existingLogFiles = Directory.GetFiles(directoryName, basicTraceFileNameWithoutExtension + "*");

                int highestNumber = -1;
                foreach (string existingLogFile in existingLogFiles)
                {
                    Match match = _logfileSuffixExpression.Match(existingLogFile);

                    int tempInt;
                    if (match.Groups.Count >= 1 &&
                        int.TryParse(match.Groups[LogFileNumberCaptureName].Value, out tempInt) &&
                        tempInt >= highestNumber)
                    {
                        highestNumber = tempInt;
                    }

                }

                return highestNumber.ToString();
            }

            return "1";
        }

        public override int GetPadlengthSuffix()
        {
            return 4;
        }
    }
}
