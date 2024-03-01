using BPMSoft.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BpmSoftIntegration
{
	public class DbManager<T> where T : Entity
	{
		protected readonly string ConnectionString;
		protected readonly string TableName;

		protected virtual string GetCommaFields(PropertyInfo[] fields)
		{
			var names = new StringBuilder();
			foreach (var field in fields)
			{
				names.Append($"{field.Name},");
			}
			names.Remove(names.Length - 1, 1);
			return names.ToString();
		}

		protected virtual bool IsExistsWithId(Guid id)
		{
			if (id == Guid.Empty)
			{
				throw new ArgumentNullOrEmptyException(nameof(id));
			}

			var idParam = "@Id";

			var query = $"SELECT COUNT(Id) FROM {TableName} WHERE Id={idParam};";
			var command = new SqlCommand(query);
			command.Parameters.AddWithValue(idParam, id);

			using var connection = new SqlConnection(ConnectionString);
			connection.Open();
			command.Connection = connection;

			var reader = command.ExecuteScalar();
			if (reader == null || reader == DBNull.Value)
			{
				return false;
			}

			return (int)reader > 0;
		}

		protected virtual Dictionary<string, string> GetQueryParametersMap(T entity, List<string>? includedColumns = default)
		{
			var fields = entity.Fields;

			var result = new Dictionary<string, string>();
			if (includedColumns != null)
			{
				var i = 0;
				var j = 0;
				while (j < includedColumns.Count && i < fields.Length)
				{
					if (includedColumns.Contains(fields[i].Name))
					{
						result.Add($"@A{i}", fields[i].Name);
						j++;
					}
					i++;
				}
			}
			else
			{
				for (int i = 0; i < fields.Length; i++)
				{
					result.Add($"@A{i}", fields[i].Name);
				}
			}

			return result;
		}

		protected virtual SqlCommand BuildCreateCommand(T entity)
		{
			var queryTemplate = "INSERT INTO {0} ({1}) VALUES ({2});";
			var command = new SqlCommand();

			var parametersMap = GetQueryParametersMap(entity);
			var parametersList = new StringBuilder();
			var utcNow = DateTime.UtcNow;
			foreach (var parameterMap in parametersMap)
			{
				parametersList.Append(parameterMap.Key + ',');

				switch (parameterMap.Value)
				{
					case "ModifiedOn":
					case "CreatedOn":
						command.Parameters.AddWithValue(parameterMap.Key, utcNow);
						break;

					default:
						var property = entity.GetType().GetProperty(parameterMap.Value) ?? 
							throw new DbUserManagerException($"Property {parameterMap.Value} not found at {typeof(T).Name}");

						var value = property.GetValue(entity) ?? DBNull.Value;
						if (property.GetCustomAttribute(typeof(NotNullAttribute)) != null && value == DBNull.Value)
						{
							if (property.PropertyType == typeof(string))
							{
								value = "";
							}
							else
							{
								throw new DbUserManagerException($"Column {parameterMap.Value} must not be null");
							}
						}
						command.Parameters.AddWithValue(parameterMap.Key, value);
						break;
				}
			}

			// Remove the last comma.
			parametersList.Remove(parametersList.Length - 1, 1);

			command.CommandText = string.Format(queryTemplate, TableName, GetCommaFields(entity.Fields), parametersList);
			return command;
		}

		protected virtual SqlCommand BuildReadCommand(int rowCount, List<string> readingColumns, Dictionary<string, object>? equalFilters = null)
		{
			var query = $"SELECT TOP ({rowCount}) " +
				$"{string.Join(",", readingColumns)} " +
				$"FROM dbo.{TableName} ";
			var command = new SqlCommand(query);

			if (equalFilters == null || equalFilters.Count < 1)
			{
				command.CommandText += ';';
				return command;
			}

			var whereClauseTemplate = " WHERE {0} ";
			var whereClauseBlock = new StringBuilder();
			if (equalFilters.Count == 1)
			{
				var parameter = "@F0";
				var keyValuePair = equalFilters.First();
				whereClauseBlock.AppendFormat(whereClauseTemplate, $"{keyValuePair.Key}={parameter}");
				command.Parameters.AddWithValue(parameter, keyValuePair.Value);
				command.CommandText += whereClauseBlock.ToString();
				return command;
			}

			for (int i = 0; i < equalFilters.Count; i++)
			{
				var parameter = $"@F{i}";
				var keyValuePair = equalFilters.ElementAt(i);

				// The last element doesn't need AND clause.
				var union = i == equalFilters.Count - 1
					? ""
					: "AND";

				whereClauseBlock.Append($"{keyValuePair.Key}={parameter} {union} ");
				command.Parameters.AddWithValue(parameter, keyValuePair.Value);
			}
			command.CommandText += string.Format(whereClauseTemplate, whereClauseBlock.ToString());
			Console.WriteLine(command.CommandText);
			return command;
        }

		protected virtual SqlCommand BuildUpdateCommand(T entity, Dictionary<string, object> updatingColumns)
		{
			if (entity == null || entity.Id == Guid.Empty) {
				throw new ArgumentNullOrEmptyException("entity is null or its Id is empty");
			}
			if (updatingColumns == null || updatingColumns.Count < 1)
			{
				throw new ArgumentNullOrEmptyException("No columns to update were set");
			}

			// The modification date should be updated.
			if (!updatingColumns.ContainsKey("ModifiedOn"))
			{
				updatingColumns.Add("ModifiedOn", DateTime.UtcNow);
			}
			else
			{
				updatingColumns["ModifiedOn"] = DateTime.UtcNow;
			}

			foreach (var pair in updatingColumns)
			{
				var property = entity.GetType().GetProperty(pair.Key) ?? 
					throw new DbUserManagerException($"Property {pair.Key} not found at {typeof(T).Name}");
				var propertyType = property.PropertyType;
				if (propertyType == typeof(bool)) {
					property.SetValue(entity, Convert.ChangeType(pair.Value, typeof(bool)));
				} else if (propertyType == typeof(Guid) && Guid.TryParse(pair.Value as string, out var id)) {
					property.SetValue(entity, id);
				} else if (propertyType == typeof(Guid) && pair.Value != null && DateTime.TryParse(pair.Value.ToString(), out var date)) {
					property.SetValue(entity, date);
				} else {
					property.SetValue(entity, pair.Value);
				}
			}

			var idParam = "@Id";
			var queryTemplate = "UPDATE {0} SET {1} WHERE Id={2};";
			var parametersMap = GetQueryParametersMap(entity, [.. updatingColumns.Keys]);
			var updateSetBlock = new StringBuilder();

			var command = new SqlCommand();
			command.Parameters.AddWithValue(idParam, entity.Id);

			foreach (var keyValuePair in parametersMap)
			{
				updateSetBlock.Append($"{keyValuePair.Value}={keyValuePair.Key},");
				
				var property = entity.GetType().GetProperty(keyValuePair.Value) ??
					throw new DbUserManagerException($"Property {keyValuePair.Value} not found at {typeof(T).Name}");

				command.Parameters.AddWithValue(keyValuePair.Key, property.GetValue(entity));
			}

			// Remove the last comma.
			updateSetBlock.Remove(updateSetBlock.Length - 1, 1);
			command.CommandText = string.Format(queryTemplate, TableName, updateSetBlock.ToString(), idParam);
			return command;
		}

		protected virtual SqlCommand BuildDeleteCommand(T entity)
		{
			var idParam = "@Id";

			var queryTemplate = "DELETE FROM {0} WHERE Id = {1};";
			var command = new SqlCommand()
			{
				CommandText = string.Format(queryTemplate, TableName, idParam),
			};
			command.Parameters.AddWithValue(idParam, entity.Id);
			return command;
		}

		protected virtual T CreateEntityModel(SqlDataReader reader , List<string> readingColumns)
		{
			var model = Activator.CreateInstance<T>();
			model.LoadedFields = readingColumns;

			foreach (var columnName in readingColumns)
			{
				var ordinal = reader.GetOrdinal(columnName);

				var property = model.GetType().GetProperty(columnName) ??
					throw new DbUserManagerException($"Property {columnName} not found at {typeof(T).Name}");

				if (reader.IsDBNull(ordinal))
				{
					property.SetValue(model, null);
				}
				else
				{
					var value = reader.GetValue(reader.GetOrdinal(columnName));
					property.SetValue(model, value);
				}
			}
			return model;
		}

		public DbManager(string server, string db, string login, string password, string tableName = "")
		{
			if (string.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentNullOrEmptyException(nameof(server));
			}
			if (string.IsNullOrWhiteSpace(db))
			{
				throw new ArgumentNullOrEmptyException(nameof(db));
			}
			if (string.IsNullOrWhiteSpace(login))
			{
				throw new ArgumentNullOrEmptyException(nameof(login));
			}
			if (string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentNullOrEmptyException(nameof(password));
			}

			var template = "Data Source={0}; Initial Catalog={1}; Persist Security Info=True; MultipleActiveResultSets=True; User ID={2}; Password={3}; Pooling = true; Max Pool Size = 250; Connection Timeout=5";
			ConnectionString = string.Format(template, server, db, login, password);
			TableName = string.IsNullOrWhiteSpace(tableName)
				? typeof(T).Name
				: tableName;
		}

		public DbManager(System.Configuration.ConnectionStringSettings cs, string tableName = "")
		{
			cs.CheckArgumentNull("cs");

			ConnectionString = cs.ConnectionString;
			Console.WriteLine(ConnectionString);
			TableName = string.IsNullOrWhiteSpace(tableName)
				? typeof(T).Name
				: tableName;
		}

		public DbManager(string? cs, string tableName = "") {
			cs.CheckArgumentNull("cs");	

			ConnectionString = cs;
			Console.WriteLine(ConnectionString);

			TableName = string.IsNullOrWhiteSpace(tableName)
				? typeof(T).Name
				: tableName;
		}

		public virtual int Create(T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullOrEmptyException(nameof(entity));
			}
			if (IsExistsWithId(entity.Id))
			{
				throw new DbUserManagerException($"Entity with Id {entity.Id} already exists");
			}

			var command = BuildCreateCommand(entity);

			using var connection = new SqlConnection(ConnectionString);
			connection.Open();
			command.Connection = connection;

			try
			{
				return command.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				throw new DbUserManagerException($"Insert error: {TableName} {e.Message}", e);
			}
		}
		public virtual List<T> Read(int rowCount, List<string> readingColumns, Dictionary<string, object>? equalFilters = null)
		{
			if (rowCount < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(rowCount), rowCount, "must be greater than 1");
			}
			if (readingColumns == null || readingColumns.Count < 1)
			{
				throw new ArgumentNullOrEmptyException(nameof(readingColumns));
			}
			var list = new List<T>();

			var command = BuildReadCommand(rowCount, readingColumns, equalFilters);

			using (var connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				command.Connection = connection;

				try
				{
					var reader = command.ExecuteReader();
					while (reader.Read())
					{
						list.Add(CreateEntityModel(reader, readingColumns));
					}
				}
				catch (SqlException e)
				{
					throw new DbUserManagerException($"Read error: {TableName} {e.Message}", e);
				}
				catch (InvalidCastException e)
				{
					throw new DbUserManagerException($"Read error: {TableName} {e.Message}", e);
				}
				catch (IndexOutOfRangeException e)
				{
					throw new DbUserManagerException($"Read error: column {TableName} {e.Message} not found", e);
				}
			}

			return list;
		}

		public virtual int Update(T entity, Dictionary<string, object> updatingColumns)
		{
			if (entity.Id == default) {
				throw new DbUserManagerException("Can't update a record if its Id is not set");
			}

			if (entity == null)
			{
				throw new ArgumentNullOrEmptyException(nameof(entity));
			}
			if (updatingColumns == null || updatingColumns.Count < 1)
			{
				throw new ArgumentNullOrEmptyException(nameof(updatingColumns));
			}
			if (!IsExistsWithId(entity.Id))
			{
				throw new DbUserManagerException($"Entity with Id {entity.Id} not exists");
			}

			var command = BuildUpdateCommand(entity, updatingColumns);

			using var connection = new SqlConnection(ConnectionString);
			connection.Open();
			command.Connection = connection;

			try
			{
				return command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				throw new DbUserManagerException($"Update error: {TableName} {e.Message}", e);
			}
		}
		public virtual int Delete(T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullOrEmptyException(nameof(entity));
			}

			var command = BuildDeleteCommand(entity);

			using var connection = new SqlConnection(ConnectionString);
			connection.Open();
			command.Connection = connection;
			try
			{
				return command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				throw new DbUserManagerException($"Delete error: {TableName} {e.Message}", e);
			}
		}
	}
}