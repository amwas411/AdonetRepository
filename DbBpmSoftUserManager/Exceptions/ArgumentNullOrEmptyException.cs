using System;

namespace BpmSoftIntegration
{
	public class ArgumentNullOrEmptyException : ArgumentException
	{
		public ArgumentNullOrEmptyException(string argumentName)
			: base(string.Format("{0} is null or empty", argumentName))
		{
		}
		public ArgumentNullOrEmptyException(string argumentName, Exception innerException)
			: base(string.Format("{0} is null or empty", argumentName), innerException)
		{
		}
	}
}
