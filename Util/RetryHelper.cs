using System;

namespace DasBackupTool.Util
{
    public delegate void ExceptionOccuredEventHandler(object sender, ExceptionOccuredEventArgs e);

    public class ExceptionOccuredEventArgs : EventArgs
    {
        private Exception exception;
        private ExceptionAction action;

        public ExceptionOccuredEventArgs(Exception exception)
        {
            this.exception = exception;
        }

        public Exception Exception
        {
            get { return exception; }
        }

        public ExceptionAction Action
        {
            get { return action; }
            set { action = value; }
        }
    }

    public enum ExceptionAction { IGNORE, RETRY }

    public delegate void RetryHandler(params object[] args);
    public delegate void IgnoreHandler();

    public class RetryHelper
    {
        public event ExceptionOccuredEventHandler ExceptionOccured;

        public void Retry(RetryHandler retryHandler, params object[] args)
        {
            Retry(retryHandler, null, args);
        }

        public void Retry(RetryHandler retryHandler, IgnoreHandler ignoreHandler, params object[] args)
        {
            bool retry = true;

            while (retry)
            {
                try
                {
                    retryHandler(args);
                    retry = false;
                }
                catch (Exception e)
                {
                    retry = NotifyExceptionOccured(e);
                    if (!retry && ignoreHandler != null)
                    {
                        ignoreHandler();
                    }
                }
            }
        }

        private bool NotifyExceptionOccured(Exception exception)
        {
            ExceptionOccuredEventArgs eventArgs = new ExceptionOccuredEventArgs(exception);

            if (ExceptionOccured != null)
            {
                ExceptionOccured(this, eventArgs);
            }

            return eventArgs.Action == ExceptionAction.RETRY;
        }
    }
}
