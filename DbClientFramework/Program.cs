using System;
using System.Collections.Generic;
using System.Linq;
using BpmSoftIntegration;

namespace DbClientFramework
{
	internal class Program
	{
		static void Main()
		{	
			var cs = System.Configuration.ConfigurationManager.ConnectionStrings["db"];
		
			var manager = new UserManager(cs);
			SysAdminUnit? lastSelectedUser = null;

			do
			{
				try
				{
					Console.WriteLine("C/R/U/D");

					var k = Console.ReadKey().Key;
					switch (k)
					{
						case ConsoleKey.Escape:
							Console.WriteLine("Exited");
							return;

						case ConsoleKey.C:
							Console.WriteLine("Create: Column1:Value1 ... ColumnN:ValueN");
							var prompt = Console.ReadLine();
							if (string.IsNullOrWhiteSpace(prompt))
							{
								Console.WriteLine($"Prompt length must be in range [1:N], but got null");
								continue;
							}
							var p = prompt.Split(' ');
							if (p.Length < 1)
							{
								Console.WriteLine($"Prompt length must be in range [1;N], but got {p.Length}");
								continue;
							}

							var unit = new SysAdminUnit
							{
								Id = Guid.NewGuid()
							};

							foreach (var pair in p)
							{
								var p1 = pair.Split(':');
								var v = p1[1];
								var property = unit.GetType().GetProperty(p1[0]) ?? 
									throw new DbUserManagerException($"Property {p1[0]} is not found");
								var t = property.PropertyType;
								switch(p1[0])
								{
									case "UserPassword":
										v = manager.GenerateHash(p1[1]);
										property.SetValue(unit, Convert.ChangeType(v, t));
										break;
									case "SysCultureId":
										if (string.Equals(p1[1], "ru", StringComparison.OrdinalIgnoreCase)) {
											property.SetValue(unit, Constants.SysCulture.Ru);
										}
										break;
									default:
										if (t == typeof(Guid) || t == typeof(Guid?))
										{
											property.SetValue(unit, new Guid(v));
										} 
										else if (t == typeof(DateTime) || t == typeof(DateTime?))
										{
											property.SetValue(unit, DateTime.Parse(v));
										}
										else
										{
											property.SetValue(unit, Convert.ChangeType(v, t));
										}
										break;
								}
							}

							Console.WriteLine("Rows affected: " + manager.Create(unit, Constants.SysRole.AllEmployees));
							lastSelectedUser = unit;
							break;

						case ConsoleKey.R:
							Console.WriteLine("Read: RowCount Column1,...,ColumnN [FilterColumn1:FilterValue1,...,FilterColumnM:FilterValueM]");
							prompt = Console.ReadLine();
							var filter = new Dictionary<string, object>();
							var rowCount = 0;
							var cols = new List<string>();
							if (string.IsNullOrWhiteSpace(prompt))
							{
								Console.WriteLine($"Prompt length must be in range [2;3], but got null");
								continue;
							}

							if (!string.IsNullOrWhiteSpace(prompt))
							{
								p = prompt.Split(' ');
								if (p.Length < 2 || p.Length > 3) 
								{
									Console.WriteLine($"Prompt length must be in range [2;3], but got {p.Length}");
									continue;
								}

								rowCount = Convert.ToInt32(p[0]);

								cols = [.. p[1].Split(',')];
								
								if (p.Length == 3)
								{
									var f = p[2];
									var pairs = f.Split(',');
                                    foreach (var pair in pairs)
                                    {
										var a = pair.Split(':');
										filter.Add(a[0], a[1]);
                                    }
								}
							}
							var sysAdminUnits = manager.Read(rowCount, cols, filter);
							foreach (var item in sysAdminUnits)
							{
								Console.WriteLine(item);
							}
							lastSelectedUser = sysAdminUnits.LastOrDefault();
							break;

						case ConsoleKey.U:
							if (lastSelectedUser == null) 
							{
								Console.WriteLine("User wasn't selected");
								break;
							}
							Console.WriteLine("Update: Column1:Value1 ... ColumnM:ValueM");
							prompt = Console.ReadLine();

							if (string.IsNullOrWhiteSpace(prompt))
							{
								Console.WriteLine($"Prompt length must be in range [1;N], but got null");
								continue;
							}


							p = prompt.Split(' ');
							if (p.Length < 1) { Console.WriteLine("Prompt length must be in range [1;N]"); }

							var columnValues = new Dictionary<string, object>();

							foreach (var pair in p)
							{
								var a = pair.Split(':');
								var v = a[1];
								if (a[0] == "UserPassword")
								{
									v = manager.GenerateHash(a[1]);
								}
								columnValues.Add(a[0], v);
							}

							Console.WriteLine("Rows affected: " + manager.Update(lastSelectedUser, columnValues));
							break;
							
						case ConsoleKey.D:
							if (lastSelectedUser == null)
							{
								Console.WriteLine("User wasn't selected");
								break;
							}
							Console.WriteLine("Delete");
							Console.WriteLine("Rows affected: " + manager.Delete(lastSelectedUser));
							lastSelectedUser = null;
							break;

						default:
							Console.WriteLine("Unknown command");
							break;
					}
				}
				catch (ArgumentException e)
				{
					Console.WriteLine(e);
				}
				catch (DbUserManagerException e)
				{
					Console.WriteLine(e);
				}
			} while (true);
		}
	}
}