using System;
using System.IO;
using System.ServiceProcess;
using System.Collections.Generic;
using Microsoft.Win32;

namespace SharpSvc
{
	class SharpSvc
	{
		static void Main(string[] args)
		{
			if (args == null || args.Length < 1)
			{
				printUsage();
			}

			if ((args[0] == "--ListSvc") && (args.Length == 3))
			{
				string Computer = args[1];
				string State = args[2];
				ListSvc(Computer, State);
			}
			else if ((args[0] == "--GetSvc") && (args.Length == 4))
			{
				string Computer = args[1];
				string ServiceName = args[2];
				string Function = args[3];
				GetSvc(Computer, ServiceName, Function);
			}
			else if ((args[0] == "--AddSvc") && (args.Length == 6))
			{
				string Computer = args[1];
				string ServiceName = args[2];
				string DisplayName = args[3];
				string Description = args[4];
				string BinaryPathName = args[5];
				AddSvc(Computer, ServiceName, DisplayName, Description, BinaryPathName);
			}
			else if ((args[0] == "--RemoveSvc") && (args.Length == 3))
			{
				string Computer = args[1];
				string ServiceName = args[2];
				RemoveSvc(Computer, ServiceName);
			}
			else
			{
				printUsage();
			}
		}

		//static void printUsage()
		//{
		//	Console.WriteLine("\n[-] Usage: \n\t--ListSvc <Computer|local|hostname|ip> <State|all|running|stopped>" +
		//		"\n\t--GetSvc <Computer|local|hostname|ip> <ServiceName|Spooler> <Function|list|stop|start>" +
		//		"\n\t--AddSvc <Computer|local|hostname|ip> <Name|VerifiedPublisherSmartScreenCheck> <DisplayName|\"Verified Publisher SmartScreen Check\">" +
		//		"<Description|\"Inspects the AppID cache for invalid SmartScreen entries.\"> <ExecutablePath|C:\\Windows\\notepad.exe> <ExecutableArgs|Optional>" +
		//		"\n\n\t--RemoveSvc <Computer|local|hostname|ip> <Name|VerifiedPublisherSmartScreenCheck>\n");
		//	System.Environment.Exit(1);
		//}

		static void printUsage()
		{
			Console.WriteLine("\n[-] Usage: \n\t--ListSvc <Computer|local|hostname|ip> <State|all|running|stopped>" +
				"\n\t--GetSvc <Computer|local|hostname|ip> <ServiceName|Spooler> <Function|list|stop|start>\n");
			System.Environment.Exit(1);
		}

		static void ListSvc(string Computer, string State)
		{
			ServiceController[] scServices;
			if (Computer.ToLower() == "local")
			{
				Computer = ".";
			}
			scServices = ServiceController.GetServices(Computer);
			List<string> ServiceList = new List<string>();
			foreach (ServiceController scTemp in scServices)
			{
				if (State.ToLower() == "all")
				{
					ServiceList.Add("\t" + scTemp.DisplayName + "," + scTemp.ServiceName + "," + scTemp.StartType);
				}
				else if (State.ToLower() == "running")
				{
					if (scTemp.Status == ServiceControllerStatus.Running)
					{
						ServiceList.Add("\t" + scTemp.DisplayName + "," + scTemp.ServiceName + "," + scTemp.StartType);
					}
				}
				else if (State.ToLower() == "stopped")
				{
					if (scTemp.Status == ServiceControllerStatus.Stopped)
					{
						ServiceList.Add("\t" + scTemp.DisplayName + "," + scTemp.ServiceName + "," + scTemp.StartType);
					}
				}
				else
				{
					printUsage();
				}
			}
			ServiceList.Sort();
			foreach (string Entry in ServiceList)
			{
				Console.WriteLine(Entry);
			}
		}
		static void GetSvc(string Computer, string ServiceName, string Function)
		{
			if (Computer.ToLower() == "local")
			{
				Computer = ".";
			}
			ServiceController sc = new ServiceController(ServiceName, Computer);
			if (Function.ToLower() == "list")
			{
				Console.WriteLine("\n\tServiceName: {0}\n\tDisplayName: {1}\n\tMachineName: {2}\n\tServiceType: {3}\n\tStartType: {4}\n\tStatus: {5}\n", sc.ServiceName, sc.DisplayName, sc.MachineName, sc.ServiceType, sc.StartType, sc.Status);
			}
			else if (Function.ToLower() == "start")
			{
				Console.WriteLine("The {0} service status is currently set to {1}", sc.ServiceName, sc.Status);
				if (sc.Status == ServiceControllerStatus.Stopped)
				{
					Console.WriteLine("Starting the {0} service...", sc.ServiceName);
					try
					{
						sc.Start();
						sc.WaitForStatus(ServiceControllerStatus.Running);

						Console.WriteLine("The {0} service status is now set to {1}", sc.ServiceName, sc.Status);

					}
					catch (Exception e)
					{
						Console.WriteLine("Could not start the {0} service.", sc.ServiceName);
					}
				}
			}
			else if (Function.ToLower() == "stop")
			{
				Console.WriteLine("The {0} service status is currently set to {1}", sc.ServiceName, sc.Status);
				if (sc.Status == ServiceControllerStatus.Running)
				{
					Console.WriteLine("Stopping the {0} service...", sc.ServiceName);
					try
					{
						sc.Stop();
						sc.WaitForStatus(ServiceControllerStatus.Stopped);

						Console.WriteLine("The {0} service status is now set to {1}", sc.ServiceName, sc.Status);

					}
					catch (Exception e)
					{
						Console.WriteLine("Could not stop the {0} service.", sc.ServiceName);
					}
				}
			}
			else if (Function.ToLower() == "enable")
			{
				//
				// Uses SMB to read the registry and requires a previously authenticated session using explicit credentials. 
				//
				Console.WriteLine("The {0} service mode is currently set to {1}", sc.ServiceName, sc.StartType);
				if (sc.StartType != ServiceStartMode.Automatic)
				{
					Console.WriteLine("Enabling the {0} service...", sc.ServiceName);
					try
					{
						var key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Computer, RegistryView.Registry32).OpenSubKey(@"SYSTEM\CurrentControlSet\Services" + ServiceName);
						if (key != null)
						{
							key.SetValue("Start", 2);
						}
					}
					catch (Exception e)
					{
						throw new Exception("Could not enable the service, error: " + e.Message);
					}

					Console.WriteLine("The {0} service status is now set to {1}", sc.ServiceName, sc.Status);
				}
			}
			else
			{
				printUsage();
			}
		}
		static void ModSvc(string Computer, string ServiceName)
		{
			// Plans for functions to modify a service. ServiceController() does not support this, may require WMI or registry writes.
		}
		static void AddSvc(string Computer, string ServiceName, string DisplayName, string Description, string BinaryPathName)
		{
			// Plans for functions to create a service. ServiceController() does not support this, may require WMI or registry writes.
		}
		static void RemoveSvc(string Computer, string ServiceName)
		{
			RegistryKey environmentKey;
			try
			{
				//
				// Uses SMB to access the registry and requires a previously authenticated session using explicit credentials. 
				//
				environmentKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Computer, RegistryView.Registry32).OpenSubKey(@"SYSTEM\CurrentControlSet\Services");
				environmentKey.DeleteSubKey(ServiceName);
				//foreach (string Key in environmentKey.GetSubKeyNames())
				//{
				//	Console.WriteLine(Key);
				//}
				environmentKey.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("{0}: {1}", e.GetType().Name, e.Message);
				return;
			}
			environmentKey.Close();
		}
	}
}
