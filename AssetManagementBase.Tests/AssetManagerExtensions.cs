using System.Xml.Linq;

namespace AssetManagementBase.Tests
{
	public static class AssetManagerExtensions
	{
		private static AssetLoader<UserProfile> _userProfileLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.ReadAsString(assetName);

			var xDoc = XDocument.Parse(data);

			var result = new UserProfile
			{
				Name = xDoc.Root.Element("Name").Value,
				Score = int.Parse(xDoc.Root.Element("Score").Value)
			};

			return result;
		};

		private static AssetLoader<Job> _jobLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.ReadAsString(assetName);

			var xDoc = XDocument.Parse(data);

			var result = new Job
			{
				Title = xDoc.Root.Element("Title").Value,
				Salary = decimal.Parse(xDoc.Root.Element("Salary").Value)
			};

			return result;
		};

		private static AssetLoader<Employee> _employeeLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.ReadAsString(assetName);

			var xDoc = XDocument.Parse(data);

			var jobPath = xDoc.Root.Element("JobPath").Value;

			var result = new Employee
			{
				Name = xDoc.Root.Element("Name").Value,
				JobPath = jobPath,
				Job = manager.LoadJob(jobPath)
			};

			return result;
		};

		public static UserProfile LoadUserProfile(this AssetManager assetManager, string assetName) => assetManager.UseLoader(_userProfileLoader, assetName);

		public static Job LoadJob(this AssetManager assetManager, string assetName) => assetManager.UseLoader(_jobLoader, assetName);

		public static Employee LoadEmployee(this AssetManager assetManager, string assetName) => assetManager.UseLoader(_employeeLoader, assetName);
	}
}
