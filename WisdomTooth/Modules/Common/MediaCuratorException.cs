namespace MediaCurator.Common
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Custom exception type used to indicate that our code handles the error situation.
    /// </summary>
    [Serializable]
    public class MediaCuratorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaCuratorException"/> class.
        /// </summary>
        public MediaCuratorException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaCuratorException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public MediaCuratorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaCuratorException"/> class.
        /// </summary>
        /// <param name="errorCode">
        /// Numeric error code.
        /// </param>
        //// A passed parameter may contain leading zero, but it can be not found while searching it as a string.
        ////public MediaCuratorException(int errorCode)
        ////    : this(errorCode.ToString())
        ////{
        ////}

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaCuratorException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="inner">
        /// The inner exception.
        /// </param>
        public MediaCuratorException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaCuratorException"/> class.
        /// </summary>
        /// <param name="errorCode">
        /// Numeric error code.
        /// </param>
        /// <param name="inner">
        /// The inner exception.
        /// </param>
        public MediaCuratorException(int errorCode, Exception inner)
            : base(errorCode.ToString(), inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaCuratorException"/> class.
        /// </summary>
        /// <param name="message">
        /// Formating message.
        /// </param>
        /// <param name="args">
        /// Variable number of arguments corresponding to the formatting message.
        /// </param>
        public MediaCuratorException(string message, params object[] args)
            : base(String.Format(CultureInfo.InvariantCulture, message, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaCuratorException"/> class.
        /// This ctor is strongly suggested by Visual Studio check tools.
        /// </summary>
        /// <param name="info">
        /// The info. Do not know, what it means.
        /// </param>
        /// <param name="context">
        /// The context. Do not know, what it means.
        /// </param>
        protected MediaCuratorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Concatenates error messages of all exceptions in the exception stack.
        /// </summary>
        /// <param name="exception">
        /// The outer exception.
        /// </param>
        /// <returns>
        /// Error text.
        /// </returns>
        public static string ExceptionMessage(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            Exception ex = exception;
            while (ex != null)
            {
                if (ex != exception)
                {
                    sb.Append(" INNER: ");
                }

                sb.Append(ex.Message);
                ex = ex.InnerException;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Concatenates error messages of all exceptions in the exception stack. Creates a wrapper exception with a custom error code.
        /// </summary>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="exception">
        /// The actual exception.
        /// </param>
        /// <returns>
        /// Error text.
        /// </returns>
        public static string ExceptionMessage(int errorCode, Exception exception)
        {
           return ExceptionMessage(new MediaCuratorException(errorCode, exception));
        }
    }
}