using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Hemit.Logger
{
    /// <summary>
    /// A rolling logger that increments a logger when the size has filled up enough space defined by the 'MaxTraceFileSize' attribute.
    /// The naming convention increments the logging file name with the 'yyyyMMdd' date as the suffix and also a number of the next available
    /// log in case it is required to increment log on the same day due to filled up space condition.
    /// A rolling logger that increments a logger when the size has filled up enough space defined by the 'MaxTraceFileSize' attribute
    /// </summary>
    public class RollingDateXmlWriterTraceListener : RollingXmlWriterTraceListenerbase
    {

        /// <summary>
        /// This expression is used to find the number of a trace file in its file name by searching for an underscore (_), a
        /// numeric expression with any repetitions and a dot (that marks the beginning of the file extension). The named 
        /// capture group named by the constant &quot;LogFileNumberCaptureName&quot; will contain the number.
        /// </summary>
        /// <remarks>Example file name: OpPlanTraceLogs_20231008_0001.svclog followed by OpPlanTraceLogs_0002.svclog and so on</remarks>
        private readonly Regex _logfileSuffixExpression = new Regex(@"_(?<" + LogFileNumberCaptureName + @">\d*)\.", RegexOptions.Compiled);

        /// <summary>
        /// This is the named Regex capture group to find the numeric suffix of a trace file
        /// </summary>
        private static readonly string LogFileNumberCaptureName = "LogFileNumber";

        public RollingDateXmlWriterTraceListener(string filename) : base(filename)
        {
        }

        public RollingDateXmlWriterTraceListener(string filename, string name) : base(filename, name)
        {
        }

        protected override string GetTraceFileNameSuffix()
        {
            int highestNumber = -1;

            string directoryName = Path.GetDirectoryName(BasicTraceFileName);
            string basicTraceFileNameWithoutExtension = Path.GetFileNameWithoutExtension(BasicTraceFileName);
            if (directoryName != null)
            {
                string[] existingLogFiles = Directory.GetFiles(directoryName, basicTraceFileNameWithoutExtension + "*");

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
            }
            highestNumber = 1;
            string patternOfToday = DateTime.Today.ToString("yyyyMMdd");

            return $"{patternOfToday}_{highestNumber}";
        }

        public override int GetPadlengthSuffix()
        {
            return 12;
        }
    }
}
