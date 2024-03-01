using System;
using System.Collections.Generic;
using System.Linq;

namespace BpmSoftIntegration
{
    public class UserManager : DbManager<SysAdminUnit>
	{
		protected DbManager<SysUserInRole> DbUserInRoleManager;
		protected DbManager<Contact> DbContactManager;
		protected DbManager<SysAdminUnitInRole> DbSysAdminUnitInRoleManager;

		protected virtual bool IsExistsSysUserInRole(Guid userId, Guid userRoleId)
		{
			var result = DbUserInRoleManager.Read(1, new List<string>() { "Id" }, new Dictionary<string, object>()
			{
				{"SysRoleId", userRoleId },
				{"SysUserId", userId }
			}).FirstOrDefault();

			return result != null;
		}

		public UserManager(string server, string db, string login, string password)
			:base(server, db, login, password, "VwSysAdminUnit")
		{
			DbUserInRoleManager = new DbManager<SysUserInRole>(server, db, login, password);
			DbContactManager = new DbManager<Contact>(server, db, login, password);
			DbSysAdminUnitInRoleManager = new DbManager<SysAdminUnitInRole>(server, db, login, password);
		}

		public UserManager(System.Configuration.ConnectionStringSettings cs)
			:base(cs, "VwSysAdminUnit")
		{
			DbUserInRoleManager = new DbManager<SysUserInRole>(cs);
			DbContactManager = new DbManager<Contact>(cs);
			DbSysAdminUnitInRoleManager = new DbManager<SysAdminUnitInRole>(cs);
		}

		public UserManager(string? connectionString)
			:base(connectionString) 
		{
			DbUserInRoleManager = new DbManager<SysUserInRole>(connectionString);
			DbContactManager = new DbManager<Contact>(connectionString);
			DbSysAdminUnitInRoleManager = new DbManager<SysAdminUnitInRole>(connectionString);
		}



		public string GenerateHash(string value)
		{
			return BPMSoft.Common.PasswordCryptoProvider.GetHashByPassword(value);
		}

		public int Create(SysAdminUnit entity, Guid roleId)
		{
			var contact = new Contact()
			{
				Id = Guid.NewGuid(),
				Name = entity.Name + "_Contact"
			};
			var affected = DbContactManager.Create(contact);
			entity.ContactId = contact.Id;
			entity.Active = true;
			affected += Create(entity);
			if (!IsExistsSysUserInRole(entity.Id, roleId))
			{
				affected += DbUserInRoleManager.Create(new SysUserInRole()
				{
					Id = Guid.NewGuid(),
					SysUserId = entity.Id,
					SysRoleId = roleId
				});
				affected += DbSysAdminUnitInRoleManager.Create(new SysAdminUnitInRole()
				{
					Id = Guid.NewGuid(),
					SysAdminUnitId = entity.Id,
					SysAdminUnitRoleId = roleId,
					Source = AdminUnitRoleSources.ExplicitEntry | AdminUnitRoleSources.UpHierarchy
				});
				affected += DbSysAdminUnitInRoleManager.Create(new SysAdminUnitInRole()
				{
					Id = Guid.NewGuid(),
					SysAdminUnitId = entity.Id,
					SysAdminUnitRoleId = entity.Id,
					Source = AdminUnitRoleSources.Self
				});
			}
			return affected;
		}

		public override int Delete(SysAdminUnit entity)
		{
			if (entity.Id == default) {
				throw new DbUserManagerException("Can't delete a record if its Id is not set");
			}
			
			var userInRole = DbUserInRoleManager.Read(1, new List<string>() { "Id" }, new Dictionary<string, object>()
			{
				{"SysUserId", entity.Id }
			}).FirstOrDefault();
			var affected = 0;
			if (userInRole != null) 
			{
				affected += DbUserInRoleManager.Delete(userInRole);
			}

			affected += base.Delete(entity);
			return affected;
		}
	}
}
