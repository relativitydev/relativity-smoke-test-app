using System;

namespace SmokeTest.Exceptions
{
    public class ExceptionMessageFormatter
    {
        public static string GetInnerMostExceptionMessage(Exception exception)
        {
            string retVal;

            if (exception == null)
            {
                retVal = String.Empty;
            }
            else
            {
                Exception currentException = exception;
                while (currentException.InnerException != null)
                {
                    currentException = currentException.InnerException;
                }

                retVal = currentException.Message;
            }
            return retVal;
        }
    }
}
