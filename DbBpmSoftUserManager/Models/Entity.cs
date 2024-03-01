using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BpmSoftIntegration
{
	public abstract class Entity
	{
		[NotNull]
		public Guid Id { get; set; }
		public DateTime? CreatedOn { get; set; }
		public Guid? CreatedById { get; set; }
		public DateTime? ModifiedOn { get; set; }
		public Guid? ModifiedById { get; set; }

		[Metadata]
		public List<string>? LoadedFields { get; set; }

		[Metadata]
		public PropertyInfo[] Fields
		{
			get
			{
				_fields ??= GetType().GetProperties().Where(field =>
					{
						return field.GetCustomAttribute(typeof(LookupAttribute)) == null && field.GetCustomAttribute(typeof(MetadataAttribute)) == null;
					}).ToArray();

				return _fields;
			}
		}
		private PropertyInfo[]? _fields;

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (LoadedFields == null) {
				throw new DbUserManagerException("No loaded fields found");
			}

			foreach (var field in LoadedFields)
			{
				sb.AppendFormat("{0}\t{1}\n", field, Fields.First(i => i.Name == field).GetValue(this));
			}

			return sb.ToString();
		}
	}
}
