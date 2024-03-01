using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BpmSoftIntegration
{
	public class SysAdminUnitInRole : Entity
	{
		public AdminUnitRoleSources Source { get; set; }
		public Guid SysAdminUnitRoleId { get; set; }
		public Guid SysAdminUnitId { get; set; }
	}
}
