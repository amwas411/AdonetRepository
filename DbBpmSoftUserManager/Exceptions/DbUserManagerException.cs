using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BpmSoftIntegration
{
	public class DbUserManagerException : ApplicationException
	{
		public DbUserManagerException(string message)
			: base(message)
		{
		}
		public DbUserManagerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
