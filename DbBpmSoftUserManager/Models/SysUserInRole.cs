using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BpmSoftIntegration
{
	public class SysUserInRole : Entity
	{
		[NotNull]
		public Guid SysRoleId { get; set; }
		[NotNull]
		public Guid SysUserId { get; set; }

		public SysUserInRole() { }
	}
}
