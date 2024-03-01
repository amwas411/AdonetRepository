using System;
using System.Text;

namespace BpmSoftIntegration
{
	public class Contact : Entity
	{

		public string Name { get; set; } = "";

		public Contact(Guid id, string name) 
		{
			Id = id;
			Name = name;
		}

		public Contact() { }
	}
}
