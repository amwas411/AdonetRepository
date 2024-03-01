using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BpmSoftIntegration
{
	public static class Constants
	{
		public static class SysCulture
		{
			public static Guid Ru = new Guid("1a778e3f-0a8e-e111-84a3-00155d054c03");
			public static Guid En = new Guid("a5420246-0a8e-e111-84a3-00155d054c03");
		}

		public static class SysRole
		{
			public static Guid AllEmployees = new Guid("A29A3BA5-4B0D-DE11-9A51-005056C00008");
		}

		public static class SysAdminUnitType
		{
			public static Guid User = new Guid("472e97c7-6bd7-df11-9b2a-001d60e938c6");
			public static Guid PortalUser = new Guid("f4044c41-df2b-e111-851e-00155d04c01d");
			public static Guid Manager = new Guid("b759f1c0-6bd7-df11-9b2a-001d60e938c6");
			public static Guid Division = new Guid("b659f1c0-6bd7-df11-9b2a-001d60e938c6");
			public static Guid Organization = new Guid("df93dcb9-6bd7-df11-9b2a-001d60e938c6");
			public static Guid Team = new Guid("462e97c7-6bd7-df11-9b2a-001d60e938c6");
			public static Guid FunctionalRole = new Guid("625aca96-0293-4ab4-b7b1-37c9a6a42fed");
		}
		public static class ConnectionType
		{
			public static Guid Virtual = new Guid("e96a0adf-3e4f-47f9-81c3-e8628eab55f0");
			public static Guid Employee = new Guid("df8ff7a2-c9bf-4f28-80b1-ed16fa77818d");
			public static Guid PortalUser = new Guid("4ad0e44a-6502-4499-984c-f626d12c6301");
		}
	}

	[Flags]
	public enum AdminUnitRoleSources
	{
		/// <summary>
		/// Empty value.
		/// </summary>
		None = 0,
		/// <summary>
		/// Self.
		/// </summary>
		Self = 1,
		/// <summary>
		/// Explicit entry into role.
		/// </summary>
		ExplicitEntry = 2,
		/// <summary>
		/// Delegated role.
		/// </summary>
		Delegated = 4,
		/// <summary>
		/// Gets functional role from organisational role.
		/// </summary>
		FuncRoleFromOrgRole = 8,
		/// <summary>
		/// Gets role up hierarchy from role.
		/// </summary>
		UpHierarchy = 16,
		/// <summary>
		/// Gets role as a manager.
		/// </summary>
		AsManager = 32,
		/// <summary>
		/// Gets all roles
		/// </summary>
		All = 63
	}
}
