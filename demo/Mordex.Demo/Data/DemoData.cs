using System;
using System.Collections.Generic;
using System.Data;

namespace Mordex.Demo
{
	/// <summary>
	/// The invented data that every page shares: a small trading company with staff, customers, orders, a calendar and
	/// projects. The data is the same on every run, except that calendar and project dates follow the current week.
	/// </summary>
	public static class DemoData
	{
		private static readonly string[] FirstNames = { "Amira", "Bruno", "Chen", "Dalia", "Elias", "Farah", "Gustavo", "Hana", "Ivan", "Jun", "Katya", "Liam", "Mina", "Nils", "Olga", "Pedro", "Quinn", "Rosa", "Sami", "Tara", "Umar", "Vera", "Wen", "Yusuf", "Zoe" };

		private static readonly string[] LastNames = { "Alvarez", "Berg", "Castillo", "Dubois", "Eriksen", "Fischer", "Gupta", "Haddad", "Ito", "Jansen", "Kowalski", "Lopez", "Moreau", "Novak", "Okafor", "Petrov", "Quist", "Rossi", "Silva", "Tanaka", "Ulrich", "Varga", "Weber", "Xu", "Yilmaz" };

		public static readonly string[] Departments = { "Sales", "Marketing", "Engineering", "Support", "Finance", "Logistics" };

		private static readonly string[] Countries = { "Canada", "Germany", "Japan", "Brazil", "Spain", "Norway", "India", "Kenya" };

		private static readonly Dictionary<string, string[]> Cities = new Dictionary<string, string[]>
		{
			["Canada"] = new[] { "Toronto", "Calgary", "Montreal" },
			["Germany"] = new[] { "Berlin", "Hamburg", "Munich" },
			["Japan"] = new[] { "Osaka", "Sapporo", "Nagoya" },
			["Brazil"] = new[] { "Recife", "Curitiba", "Salvador" },
			["Spain"] = new[] { "Valencia", "Seville", "Bilbao" },
			["Norway"] = new[] { "Bergen", "Tromso", "Stavanger" },
			["India"] = new[] { "Pune", "Chennai", "Jaipur" },
			["Kenya"] = new[] { "Nairobi", "Mombasa", "Kisumu" }
		};

		private static readonly string[] Categories = { "Beverages", "Bakery", "Dairy", "Produce", "Pantry", "Seafood" };

		private static readonly string[] ProductWords = { "Roasted", "Wild", "Smoked", "Organic", "Classic", "Spiced", "Aged", "Fresh" };

		private static readonly string[] ProductNouns = { "Coffee", "Tea", "Bread", "Cheese", "Honey", "Olives", "Salmon", "Rice", "Pasta", "Apples" };

		/// <summary>The Monday of the current week. Calendar and project data are built around it.</summary>
		public static DateTime WeekStart
		{
			get
			{
				DateTime today = DateTime.Today;
				return today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
			}
		}

		/// <summary>240 employees: one flat table for sorting, grouping, filtering and totals.</summary>
		public static DataTable Employees()
		{
			Random random = new Random(11);
			DataTable table = new DataTable("Employees");
			table.Columns.Add("Id", typeof(int));
			table.Columns.Add("Name", typeof(string));
			table.Columns.Add("Department", typeof(string));
			table.Columns.Add("Title", typeof(string));
			table.Columns.Add("Country", typeof(string));
			table.Columns.Add("City", typeof(string));
			table.Columns.Add("HireDate", typeof(DateTime));
			table.Columns.Add("Salary", typeof(decimal));
			table.Columns.Add("Rating", typeof(int));
			table.Columns.Add("Active", typeof(bool));
			string[] titles = { "Associate", "Specialist", "Senior specialist", "Team lead", "Manager" };
			for (int i = 1; i <= 240; i++)
			{
				string country = Countries[random.Next(Countries.Length)];
				string[] cities = Cities[country];
				int level = random.Next(titles.Length);
				decimal salary = 42000 + level * 14000 + random.Next(0, 90) * 100;
				table.Rows.Add(
					i,
					FirstNames[random.Next(FirstNames.Length)] + " " + LastNames[random.Next(LastNames.Length)],
					Departments[random.Next(Departments.Length)],
					titles[level],
					country,
					cities[random.Next(cities.Length)],
					new DateTime(2012, 1, 1).AddDays(random.Next(0, 365 * 13)),
					salary,
					random.Next(1, 6),
					random.Next(100) > 8);
			}
			return table;
		}

		/// <summary>Customers, Orders and OrderLines with relations, for master and detail grids.</summary>
		public static DataSet Sales()
		{
			Random random = new Random(23);
			DataSet set = new DataSet("Sales");
			DataTable products = set.Tables.Add("Products");
			products.Columns.Add("ProductId", typeof(int));
			products.Columns.Add("Name", typeof(string));
			products.Columns.Add("Category", typeof(string));
			products.Columns.Add("UnitPrice", typeof(decimal));
			for (int i = 1; i <= 40; i++)
			{
				products.Rows.Add(i, ProductWords[random.Next(ProductWords.Length)] + " " + ProductNouns[random.Next(ProductNouns.Length)], Categories[random.Next(Categories.Length)], Math.Round((decimal)(random.NextDouble() * 38 + 2), 2));
			}
			DataTable customers = set.Tables.Add("Customers");
			customers.Columns.Add("CustomerId", typeof(int));
			customers.Columns.Add("Company", typeof(string));
			customers.Columns.Add("Contact", typeof(string));
			customers.Columns.Add("Country", typeof(string));
			customers.Columns.Add("City", typeof(string));
			string[] companyWords = { "Harbor", "Pine", "Summit", "Lantern", "Cedar", "Meridian", "Orchard", "Granite", "Willow", "Compass" };
			string[] companyKinds = { "Foods", "Market", "Traders", "Kitchen", "Supply", "Deli" };
			for (int i = 1; i <= 60; i++)
			{
				string country = Countries[random.Next(Countries.Length)];
				string[] cities = Cities[country];
				customers.Rows.Add(i, companyWords[random.Next(companyWords.Length)] + " " + companyKinds[random.Next(companyKinds.Length)] + " " + i, FirstNames[random.Next(FirstNames.Length)] + " " + LastNames[random.Next(LastNames.Length)], country, cities[random.Next(cities.Length)]);
			}
			DataTable orders = set.Tables.Add("Orders");
			orders.Columns.Add("OrderId", typeof(int));
			orders.Columns.Add("CustomerId", typeof(int));
			orders.Columns.Add("OrderDate", typeof(DateTime));
			orders.Columns.Add("Status", typeof(string));
			orders.Columns.Add("Freight", typeof(decimal));
			string[] statuses = { "New", "Packed", "Shipped", "Delivered", "Delivered", "Delivered" };
			DataTable lines = set.Tables.Add("OrderLines");
			lines.Columns.Add("LineId", typeof(int));
			lines.Columns.Add("OrderId", typeof(int));
			lines.Columns.Add("ProductId", typeof(int));
			lines.Columns.Add("Quantity", typeof(int));
			lines.Columns.Add("UnitPrice", typeof(decimal));
			lines.Columns.Add("Discount", typeof(double));
			lines.Columns.Add("Total", typeof(decimal), "Quantity * UnitPrice * (1 - Discount)");
			int lineId = 1;
			for (int i = 1; i <= 420; i++)
			{
				orders.Rows.Add(i, random.Next(1, 61), DateTime.Today.AddDays(-random.Next(0, 540)), statuses[random.Next(statuses.Length)], Math.Round((decimal)(random.NextDouble() * 60), 2));
				int count = random.Next(1, 6);
				for (int line = 0; line < count; line++)
				{
					DataRow product = products.Rows[random.Next(products.Rows.Count)];
					lines.Rows.Add(lineId++, i, product["ProductId"], random.Next(1, 25), product["UnitPrice"], random.Next(0, 4) * 0.05);
				}
			}
			set.Relations.Add("CustomerOrders", customers.Columns["CustomerId"], orders.Columns["CustomerId"]);
			set.Relations.Add("OrderLines", orders.Columns["OrderId"], lines.Columns["OrderId"]);
			return set;
		}

		/// <summary>A company tree with Id and ParentId, for a self-referencing grid.</summary>
		public static DataTable OrgChart()
		{
			DataTable table = new DataTable("OrgChart");
			table.Columns.Add("Id", typeof(int));
			table.Columns.Add("ParentId", typeof(int));
			table.Columns.Add("Unit", typeof(string));
			table.Columns.Add("Lead", typeof(string));
			table.Columns.Add("Headcount", typeof(int));
			table.Columns.Add("Budget", typeof(decimal));
			table.Rows.Add(1, DBNull.Value, "Harbor & Pine Trading", "Amira Alvarez", 240, 18200000m);
			string[] leads = { "Bruno Berg", "Chen Castillo", "Dalia Dubois", "Elias Eriksen", "Farah Fischer", "Gustavo Gupta" };
			int id = 2;
			for (int d = 0; d < Departments.Length; d++)
			{
				int departmentId = id++;
				table.Rows.Add(departmentId, 1, Departments[d], leads[d], 30 + d * 6, 1400000m + d * 310000m);
				for (int team = 1; team <= 3; team++)
				{
					int teamId = id++;
					table.Rows.Add(teamId, departmentId, Departments[d] + " team " + team, FirstNames[(d * 3 + team) % FirstNames.Length] + " " + LastNames[(d * 5 + team) % LastNames.Length], 8 + team * 2, 280000m + team * 40000m);
					for (int squad = 1; squad <= 2; squad++)
					{
						table.Rows.Add(id++, teamId, Departments[d] + " " + team + "." + squad, FirstNames[(d + team + squad * 7) % FirstNames.Length] + " " + LastNames[(d * 2 + team + squad) % LastNames.Length], 3 + squad, 90000m + squad * 15000m);
					}
				}
			}
			return table;
		}

		public static readonly string[] Staff = { "Amira Alvarez", "Bruno Berg", "Chen Castillo", "Dalia Dubois", "Elias Eriksen" };

		public sealed class CalendarEvent
		{
			public DateTime Start;
			public DateTime End;
			public string Subject;
			public string Location;
			public string Owner;
		}

		/// <summary>Meetings, visits and reviews for the current and next week, for five people.</summary>
		public static IList<CalendarEvent> CalendarEvents()
		{
			DateTime monday = WeekStart;
			Random random = new Random(5);
			string[] subjects = { "Design review", "Customer call", "Sprint planning", "Supplier visit", "Budget check", "Training", "One to one", "Release sync", "Inventory count", "Demo for partners" };
			string[] places = { "Room A", "Room B", "Video call", "Warehouse", "Client office" };
			List<CalendarEvent> events = new List<CalendarEvent>();
			for (int day = 0; day < 14; day++)
			{
				DateTime date = monday.AddDays(day);
				if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
				{
					continue;
				}
				foreach (string owner in Staff)
				{
					int count = random.Next(1, 4);
					int hour = 8;
					for (int i = 0; i < count; i++)
					{
						hour += random.Next(0, 3);
						int length = random.Next(1, 4);
						if (hour + length > 18)
						{
							break;
						}
						events.Add(new CalendarEvent
						{
							Start = date.AddHours(hour).AddMinutes(random.Next(0, 2) * 30),
							End = date.AddHours(hour + length),
							Subject = subjects[random.Next(subjects.Length)],
							Location = places[random.Next(places.Length)],
							Owner = owner
						});
						hour += length;
					}
				}
			}
			return events;
		}

		public sealed class ProjectTask
		{
			public string Project;
			public string Name;
			public string Resource;
			public DateTime Start;
			public DateTime End;
			public int Percent;
		}

		/// <summary>Four projects with tasks spread over about three months around today, for the timeline.</summary>
		public static IList<ProjectTask> ProjectTasks()
		{
			DateTime origin = WeekStart.AddDays(-14);
			string[] projects = { "Web shop launch", "Warehouse move", "Loyalty program", "Supplier portal" };
			string[][] tasks =
			{
				new[] { "Requirements", "Design", "Build catalog", "Payments", "Testing", "Go live" },
				new[] { "Site survey", "Racking order", "Pack and ship", "Set up new hall", "Count stock" },
				new[] { "Research", "Rules", "App changes", "Pilot", "Rollout" },
				new[] { "API design", "Onboarding flow", "Invoices", "Reports", "Training" }
			};
			List<ProjectTask> list = new List<ProjectTask>();
			Random random = new Random(41);
			for (int p = 0; p < projects.Length; p++)
			{
				DateTime cursor = origin.AddDays(p * 6);
				for (int t = 0; t < tasks[p].Length; t++)
				{
					int days = random.Next(5, 16);
					DateTime start = cursor.AddDays(random.Next(0, 4));
					DateTime end = start.AddDays(days);
					int percent = end < DateTime.Today ? 100 : start > DateTime.Today ? 0 : (int)(100.0 * (DateTime.Today - start).TotalDays / days);
					list.Add(new ProjectTask { Project = projects[p], Name = tasks[p][t], Resource = Staff[(p + t) % Staff.Length], Start = start, End = end, Percent = percent });
					cursor = end;
				}
			}
			return list;
		}
	}
}
