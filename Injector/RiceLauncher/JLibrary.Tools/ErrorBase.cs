using System;

namespace JLibrary.Tools
{
	[Serializable]
	public abstract class ErrorBase
	{
		protected Exception _lasterror;

		public virtual Exception GetLastError()
		{
			return this._lasterror;
		}

		public virtual void ClearErrors()
		{
			this._lasterror = null;
		}

		protected virtual bool SetLastError(Exception e)
		{
			this._lasterror = e;
			return false;
		}

		protected virtual bool SetLastError(string message)
		{
			return this.SetLastError(new Exception(message));
		}
	}
}
