using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BpmSoftIntegration
{
	public class SysAdminUnit : Entity
	{
		[NotNull]
		public string? Name { get; set; }
		[NotNull]
		public string? Email { get; set; }
		[NotNull]
		public string? Description { get; set; }
		public Guid? ParentRoleId { get; set; }
		public Guid? ContactId { get; set; }
		[NotNull]
		public bool IsDirectoryEntry { get; set; }
		public Guid? TimeZoneId { get; set; }
		[NotNull]
		public string? UserPassword { get; set; }
		public Guid? SysAdminUnitTypeId { get; set; }
		public Guid? AccountId { get; set; }
		[NotNull]
		public bool Active { get; set; }
		[NotNull] 
		public bool LoggedIn { get; set; }
		[NotNull]
		public bool SynchronizeWithLDAP { get; set; }
		public string? LDAPEntry { get; set; }
		public string? LDAPEntryId { get; set; }
		public string? LDAPEntryDN { get; set; }
		[NotNull]
		public Guid? SysCultureId { get; set; }
		[NotNull]
		public int ProcessListeners { get; set; }
		public DateTime? PasswordExpireDate { get; set; }
		public Guid? HomePageId { get; set; }
		[NotNull]
		public int ConnectionType { get; set; }
		public Guid? UserConnectionTypeId { get; set; }
		[NotNull]
		public bool ForceChangePassword { get; set; }
		public Guid? LDAPElementId { get; set; }
		public Guid? DateTimeFormatId { get; set; }
		public Guid? SysAdminUnitId { get; set; }
		[NotNull]
		public int SessionTimeout { get; set; }

		public SysAdminUnit() { }
	}
}
