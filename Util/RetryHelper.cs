using System;

namespace DasBackupTool.Util
{
    public delegate void ExceptionOccuredEventHandler(object sender, ExceptionOccuredEventArgs e);

    public class ExceptionOccuredEventArgs : EventArgs
    {
        private Exception exception;
        private int retryCount;
        private bool retry = true;

        public ExceptionOccuredEventArgs(Exception exception, int retryCount)
        {
            this.exception = exception;
            this.retryCount = retryCount;
        }

        public Exception Exception
        {
            get { return exception; }
        }

        public int RetryCount
        {
            get { return retryCount; }
        }

        public bool Retry
        {
            get { return retry; }
            set { retry = value; }
        }
    }

    public delegate void RetryHandler(params object[] args);

    public class RetryHelper
    {
        public event ExceptionOccuredEventHandler ExceptionOccured;

        public bool Retry(RetryHandler retryHandler, params object[] args)
        {
            bool retry = true;
            int retryCount = -1;

            while (retry)
            {
                retryCount++;
                try
                {
                    retryHandler(args);
                    return true;
                }
                catch (Exception e)
                {
                    retry = NotifyExceptionOccured(e, retryCount);
                }
            }
            return false;
        }

        private bool NotifyExceptionOccured(Exception exception, int retryCount)
        {
            ExceptionOccuredEventArgs eventArgs = new ExceptionOccuredEventArgs(exception, retryCount);

            if (ExceptionOccured != null)
            {
                ExceptionOccured(this, eventArgs);
            }

            return eventArgs.Retry;
        }
    }
}
