using ICSharpCode.SharpZipLib.Zip;
#if NETCOREAPP
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace ICSharpCode.SharpZipLib.ReproTester.Repros
{
	public class Repro194 : Repro
	{
#if !NETCOREAPP
		internal override void Run()
		{
			ConsoleWriter.Write("ASP.NET CORE testing is not supported on .NET Framework");
		}
#else
		internal static string[] ZipFileList { get; set; }

		public Repro194()
		{
			if (ZipFileList == null)
			{
				ZipFileList = GetDummyFiles(5);
			}
		}

		internal override void Run()
		{
			CreateWebHostBuilder().Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder() =>
			WebHost.CreateDefaultBuilder()
				.UseStartup<Startup>();

		private class Startup
		{
			public void ConfigureServices(IServiceCollection services)
				=> services.AddMvc()
					.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			public void Configure(IApplicationBuilder app)
				=> app.UseMvc();
		}
	}

	[Route("api/[controller]")]
	[ApiController]
	public class Repro194Controller : ControllerBase
	{
		[Route("")]
		public IActionResult DownloadZipToBrowser()
		{
			byte[] buffer = new byte[4096];

			var ms = new MemoryStream();
			using (var zipOutputStream = new ZipOutputStream(ms))
			{
				zipOutputStream.IsStreamOwner = false;
				zipOutputStream.SetLevel(3); //0-9, 9 being the highest level of compression

				foreach (string fileName in Repro194.ZipFileList)
				{

					using (var fs = System.IO.File.OpenRead(fileName))    // or any suitable inputstream
					{

						var entry = new ZipEntry(ZipEntry.CleanName(fileName));
						entry.Size = fs.Length;
						// Setting the Size provides WinXP built-in extractor compatibility,
						//  but if not available, you can set zipOutputStream.UseZip64 = UseZip64.Off instead.

						zipOutputStream.PutNextEntry(entry);

						int count = fs.Read(buffer, 0, buffer.Length);
						while (count > 0)
						{
							zipOutputStream.Write(buffer, 0, count);
							count = fs.Read(buffer, 0, buffer.Length);
							//if (!Response.IsClientConnected) break;

							ms.Flush();
						}
					}
				}

				zipOutputStream.Flush();

			}
			ms.Flush();
			ms.Seek(0, SeekOrigin.Begin);

			return File(ms, "application/zip", "Download.zip");
		}
#endif
	}
}
