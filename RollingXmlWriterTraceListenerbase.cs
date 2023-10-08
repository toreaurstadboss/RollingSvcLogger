// <copyright file="RollingXmlWriterTraceListener.cs" license="GNU Lesser General Public License v3" />
// <author>Marc Wittke</author>
// <email>marc@wittke-web.SpamBotGoAway.de</email>
// <date>2008-11-13</date>
// <summary>Contains the RollingXmlWriterTraceListener class.</summary>
// <remarks>Code tidy-up by Tore Aurstad, tore.aurstad@hemit.no</remarks> 
namespace Hemit.Logger
{
    #region usings
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    #endregion

    /// <summary>
    /// An extended XmlWriterTraceListener that starts a new file after a configured trace file size. Trace files will be numbered with a four character number suffix.
    /// <example>
    ///     <code>
    ///         <sharedListeners>
    ///             <!-- Adding RollingXmlWriterTraceListener and setting the output folder to C:\svclogs and the Max trace file size to 100 MB -->
    ///             <add type="Hemit.Logger.RollingXmlWriterTraceListener, Hemit.Logger" name="System.ServiceModel.XmlTrace.Listener" traceOutputOptions="None" initializeData="C:\svclogs\MyTraceLogFile.svclog" MaxTraceFileSize="104857600" />
    ///         </sharedListeners>
    ///     </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// To customize the attributes the logger should support, override the method <see cref="GetSupportedAttributes"/>
    /// Default max trace file size per log file in the rolling log is default set to 128 MB. This is overriden with the 'MaxTraceFileSize' attribute
    /// </remarks>
    [HostProtection(Synchronization = true)]
    public abstract class RollingXmlWriterTraceListenerbase : XmlWriterTraceListener
    {
        #region private fields
      
        /// <summary>
        /// This field will be used to remember whether or not we have loaded the custom attributes from the config yet. The 
        /// initial value is, of course, false.
        /// </summary>
        private bool _attributesLoaded;

     
        /// <summary>
        /// The current numeric suffix for trace file names
        /// </summary>
        protected string CurrentFileNameSuffix;

        /// <summary>
        /// The size in bytes of a trace file before a new file is started. The default value is 128 Mbytes
        /// </summary>
        private long _maxTraceFileSize = 128 * 1024 * 1024;

        /// <summary>
        /// The basic trace file name as it is configured in configuration file's system.diagnostics section. However, this
        /// class will append a numeric suffix to the file name (respecting the original file extension).
        /// </summary>
        private readonly string _basicTraceFileName;

        /// <summary>
        /// Adjust the state of logger when increment the trace file
        /// </summary>
        protected virtual void IncrementTraceFileNameSuffix()
        {
        }

        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RollingXmlWriterTraceListenerbase"/> class by specifying the trace file
        /// name.
        /// </summary>
        /// <param name="filename">The trace file name.</param>
        public RollingXmlWriterTraceListenerbase(string filename)
            : base(filename)
        {
            _basicTraceFileName = filename;
            CurrentFileNameSuffix = GetTraceFileNameSuffix();
        }

        /// <summary>
        /// The basic trace file name as it is configured in configuration file's system.diagnostics section. However, this
        /// class will append a numeric suffix to the file name (respecting the original file extension).
        /// </summary>
        protected string BasicTraceFileName => _basicTraceFileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollingXmlWriterTraceListenerbase"/> class by specifying the trace file
        /// name and the name of the new instance.
        /// </summary>
        /// <param name="filename">The trace file name.</param>
        /// <param name="name">The name of the new instance.</param>
        public RollingXmlWriterTraceListenerbase(string filename, string name)
            : base(filename, name)
        {
            _basicTraceFileName = filename;
        }
        #endregion

        #region properties
        /// <summary>
        /// Gets the name of the current trace file. It is combined from the configured trace file plus an increasing number
        /// </summary>
        /// <value>The name of the current trace file.</value>
        public string CurrentTraceFileName
        {
            get
            {
                string dirName = Path.GetDirectoryName(_basicTraceFileName);
                if (dirName != null)
                    return Path.Combine(
                        dirName,
                        $"{Path.GetFileNameWithoutExtension(_basicTraceFileName)}_{CurrentFileNameSuffix.ToString().PadLeft(GetPadlengthSuffix(), '0')}{Path.GetExtension(_basicTraceFileName)}");
                return _basicTraceFileName;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size of the trace file.
        /// </summary>
        /// <value>The maximum size of the trace file.</value>
        // ReSharper disable once MemberCanBePrivate.Global
        public long MaxTraceFileSize
        {
            get
            {
                if (!_attributesLoaded)
                {
                    LoadAttributes();
                }

                return _maxTraceFileSize;
            }

            // ReSharper disable once UnusedMember.Global
            set
            {
                if (!_attributesLoaded)
                {
                    LoadAttributes();
                }

                _maxTraceFileSize = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the condition to roll over the trace file is reached.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the condition to roll over the trace file is reached; otherwise, <c>false</c>.
        /// </value>
        protected bool IsRollingConditionReached
        {
            get
            {
                // go down to the file stream
                StreamWriter streamWriter = (StreamWriter)Writer;
                FileStream fileStream = (FileStream)streamWriter.BaseStream;
                string traceFileName = fileStream.Name;

                FileInfo traceFileInfo = new FileInfo(traceFileName);

                if (traceFileInfo.Length > MaxTraceFileSize)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Emits an error message to the listener.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        public override void Fail(string message)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.Fail(message);
        }

        /// <summary>
        /// Emits an error message and a detailed message to the listener.
        /// </summary>
        /// <param name="message">The error message to write.</param>
        /// <param name="detailMessage">The detailed error message to append to the error message.</param>
        public override void Fail(string message, string detailMessage)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.Fail(message, detailMessage);
        }

        /// <summary>
        /// Writes trace information, a data object, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">A data object to emit.</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <summary>
        /// Writes trace information, data objects, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">An array of data objects to emit.</param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.TraceData(eventCache, source, eventType, id, data);
        }

        /// <summary>
        /// Writes trace and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <PermissionSet>
        ///     <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        ///     <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode"/>
        /// </PermissionSet>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.TraceEvent(eventCache, source, eventType, id);
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The message to write.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.TraceEvent(eventCache, source, eventType, id, message);
        }

        /// <summary>
        /// Writes trace information, a formatted message, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType"/> values.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more format items that correspond to objects in the <paramref name="args"/> array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }

        /// <summary>
        /// Writes trace information including the identity of a related activity, a message, and event information to the file or stream.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache"/> that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">The source name.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A trace message to write.</param>
        /// <param name="relatedActivityId">A <see cref="T:System.Guid"/> structure that identifies a related activity.</param>
        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.TraceTransfer(eventCache, source, id, message, relatedActivityId);
        }

        /// <summary>
        /// Writes the value of the object's <see cref="M:System.Object.ToString"/> method to the listener.
        /// </summary>
        /// <param name="o">An <see cref="T:System.Object"/> whose fully qualified class name you want to write.</param>
        public override void Write(object o)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.Write(o);
        }

        /// <summary>
        /// Writes a category name and the value of the object's <see cref="M:System.Object.ToString"/> method to the listener.
        /// </summary>
        /// <param name="o">An <see cref="T:System.Object"/> whose fully qualified class name you want to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void Write(object o, string category)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.Write(o, category);
        }

        /// <summary>
        /// Writes a verbatim message without any additional context information to the file or stream.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void Write(string message)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.Write(message);
        }

        /// <summary>
        /// Writes a category name and a message to the listener.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void Write(string message, string category)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.Write(message, category);
        }

        /// <summary>
        /// Writes the value of the object's <see cref="M:System.Object.ToString"/> method to the listener.
        /// </summary>
        /// <param name="o">An <see cref="T:System.Object"/> whose fully qualified class name you want to write.</param>
        public override void WriteLine(object o)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.WriteLine(o);
        }

        /// <summary>
        /// Writes a category name and the value of the object's <see cref="M:System.Object.ToString"/> method to the listener.
        /// </summary>
        /// <param name="o">An <see cref="T:System.Object"/> whose fully qualified class name you want to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void WriteLine(object o, string category)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.WriteLine(o, category);
        }

        /// <summary>
        /// Writes a verbatim message without any additional context information followed by the current line terminator to the file or stream.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteLine(string message)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.WriteLine(message);
        }

        /// <summary>
        /// Writes a category name and a message to the listener, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public override void WriteLine(string message, string category)
        {
            if (IsRollingConditionReached)
            {
                StartNewTraceFile();
            }

            base.WriteLine(message, category);
        }

        #endregion

        #region protected methods
        /// <summary>
        /// Gets the custom attributes supported by the trace listener.
        /// </summary>
        /// <returns>
        /// A string array naming the custom attributes supported by the trace listener, or null if there are no custom attributes.
        /// </returns>
        protected override string[] GetSupportedAttributes()
        {
            return new[] { "MaxTraceFileSize" };
        }
        #endregion

        #region private methods
        /// <summary>
        /// Causes the writer to start a new trace file with an increased number in the file names suffix
        /// </summary>
        private void StartNewTraceFile()
        {
            // get the underlying file stream
            StreamWriter streamWriter = (StreamWriter)Writer;
            FileStream fileStream = (FileStream)streamWriter.BaseStream;

            // close it
            fileStream.Close();

            // increase the suffix number
            IncrementTraceFileNameSuffix();

            // create a new file stream and a new stream writer and pass it to the listener - here we write to next file 
            // 
            Writer = new StreamWriter(new FileStream(CurrentTraceFileName, FileMode.Create));
        }

        /// <summary>
        /// Returns the pad suffix length of file name suffixes. E.g. '1' => '0001' when padding length is set to four and so on.
        /// </summary>
        /// <returns></returns>
        public abstract int GetPadlengthSuffix();


        /// <summary>
        /// Gets the trace file name suffix by checking whether similar trace files are already existant. 
        /// The method will find the latest trace file and resolve the next trace filename
        /// </summary>
        /// <returns>The next trace file name</returns>
        protected abstract string GetTraceFileNameSuffix();
      
        /// <summary>
        /// Reads the custom attributes' values from the configuration file. We call this method the first time the attributes
        /// are accessed.
        /// <remarks>We do not do this when the listener is constructed becausethe attributes will not yet have been read 
        /// from the configuration file.</remarks>
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"></exception>
        private void LoadAttributes()
        {
            if (Attributes.ContainsKey("MaxTraceFileSize") && !String.IsNullOrEmpty(Attributes["MaxTraceFileSize"]))
            {
                long tempLong;
                string attributeValue = Attributes["MaxTraceFileSize"];

                if (long.TryParse(attributeValue, out tempLong))
                {
                    _maxTraceFileSize = long.Parse(Attributes["MaxTraceFileSize"], NumberFormatInfo.InvariantInfo);
                }
                else
                {
                    throw new ConfigurationErrorsException(String.Format("Trace listener {0} has an unparseable configuration attribute \"MaxTraceFileSize\". The value \"{1}\" cannot be parsed to a long value.",
                        Name, attributeValue));
                }
            }

            _attributesLoaded = true;
        }
        #endregion
    }
}