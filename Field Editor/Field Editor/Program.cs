using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FieldEditor
{
	/// <summary>
	/// The application entry point. Allows the application to load assemblies from embedded resources.
	/// </summary>
	public static class Program
	{
		[STAThread]
		public static void Main()
		{
			AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
			var x = new App();
			x.Run(new MainWindow());
		}

		private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
		{
			var executingAssembly = Assembly.GetExecutingAssembly();
			var assemblyName = new AssemblyName(args.Name);

			var path = assemblyName.Name + ".dll";
			if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
				path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);

			using (var stream = executingAssembly.GetManifestResourceStream(path))
			{
				if (stream == null)
					return null;

				var assemblyRawBytes = new byte[stream.Length];
				stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
				return Assembly.Load(assemblyRawBytes);
			}
		}
	}
}