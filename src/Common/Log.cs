// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Diagnostics;
using System.Security.Cryptography;

namespace HyperNebula.Common;

public static class Log
{
	private const int MAX_MESSAGE_LENGTH = 64;
	public static bool is_locked;
	
	private const int DEBUG_LEVEL = 1;

	public static void Info(string message)
	{
		if (DEBUG_LEVEL < 0)
		{
			return;
		}
		Info(message, "");
	}
	
	public static void Info(string message, string args, int level)
	{
		if (level > DEBUG_LEVEL)
		{
			return;
		}
		
		while (is_locked)
		{
			// Wait for unlock
			Thread.Sleep(1);
		}

		is_locked               = true;
		Console.ForegroundColor = ConsoleColor.Green;
		Console.Write("INFO  ");

		DateTime now = DateTime.Now;
		Console.ForegroundColor = ConsoleColor.White;
		Console.Write("[" + now.ToString("yyyy-MM-dd | HH:mm:ss.fff") + "]");

		if (message.Length >= MAX_MESSAGE_LENGTH)
		{
			message = message.Substring(0, MAX_MESSAGE_LENGTH);
		}

		Console.Write(" " + message);
		Console.Write("   ".PadLeft(MAX_MESSAGE_LENGTH - message.Length));

		Console.Write(args);

		Console.Write("\n");
		is_locked = false;
	}

	public static void Info(string message, string args)
	{
		Info(message, args, 1);
	}

	public static void Error(string message, string args, int level)
	{
		if (level <= DEBUG_LEVEL)
		{
			Error(message, args);
		}
	}

	public static void Warn(string message, string args, int level)
	{
		if (level <= DEBUG_LEVEL)
		{
			Warn(message, args);
		}
	}

	public static void Error(string message, string args)
	{
		while (is_locked)
		{
			// Wait for unlock
			Thread.Sleep(1);
		}

		is_locked               = true;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.Write("ERROR ");

		DateTime now = DateTime.Now;
		Console.ForegroundColor = ConsoleColor.White;
		Console.Write("[" + now.ToString("yyyy-MM-dd | HH:mm:ss.fff") + "]");

		if (message.Length >= MAX_MESSAGE_LENGTH)
		{
			message = message.Substring(0, MAX_MESSAGE_LENGTH);
		}

		Console.Write(" " + message);
		Console.Write("   ".PadLeft(MAX_MESSAGE_LENGTH - message.Length));

		Console.Write(args);

		Console.Write("\n");
		is_locked = false;
	}

	public static void Warn(string message, string args)
	{
		while (is_locked)
		{
			// Wait for unlock
			Thread.Sleep(1);
		}

		is_locked               = true;
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.Write("WARN  ");

		DateTime now = DateTime.Now;
		Console.ForegroundColor = ConsoleColor.White;
		Console.Write("[" + now.ToString("yyyy-MM-dd | HH:mm:ss.fff") + "]");

		if (message.Length >= MAX_MESSAGE_LENGTH)
		{
			message = message.Substring(0, MAX_MESSAGE_LENGTH);
		}

		Console.Write(" " + message);
		Console.Write("   ".PadLeft(MAX_MESSAGE_LENGTH - message.Length));

		Console.Write(args);

		Console.Write("\n");
		is_locked = false;
	}
	
	public static void Warn(string message)
	{
		while (is_locked)
		{
			// Wait for unlock
			Thread.Sleep(1);
		}

		is_locked               = true;
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.Write("WARN  ");

		DateTime now = DateTime.Now;
		Console.ForegroundColor = ConsoleColor.White;
		Console.Write("[" + now.ToString("yyyy-MM-dd | HH:mm:ss.fff") + "]");

		if (message.Length >= MAX_MESSAGE_LENGTH)
		{
			message = message.Substring(0, MAX_MESSAGE_LENGTH);
		}

		Console.Write(" " + message);

		Console.Write("\n");
		is_locked = false;
	}

	private static void lockWait()
	{
		while (is_locked)
		{
			Console.WriteLine("Log.lockWait()");

			// Sleep for a while using RNG so we don't spam the logs while multiple threads are open
			RNGCryptoServiceProvider rng = new();
			byte[] entropy = new byte[4];
			rng.GetBytes(entropy);
			int rand = BitConverter.ToUInt16(entropy, 0);
			ushort sleep_time = (ushort)(rand % 50); // modulo for a maximum of 50 ms
			Thread.Sleep(sleep_time);
		}
	}

	public static void Fatal(string message, string args)
	{
		lockWait();
		lockWait();

		is_locked               = true;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.Write("FATAL ");

		DateTime now = DateTime.Now;
		Console.ForegroundColor = ConsoleColor.White;
		Console.Write("[" + now.ToString("yyyy-MM-dd | HH:mm:ss.fff") + "]");

		if (message.Length >= MAX_MESSAGE_LENGTH)
		{
			message = message.Substring(0, MAX_MESSAGE_LENGTH);
		}

		Console.Write(" " + message);
		Console.Write("   ".PadLeft(MAX_MESSAGE_LENGTH - message.Length));

		Console.Write(args);

		Console.Write("\n");
		is_locked = false;
		ForceExit(1);
	}

	private static void ForceExit(int error_code)
	{
		// Make sure we *really* exit within the next 1 millisecond
		new Thread(KillProcess);
		
		// Normal exit. If this hangs, KillProcess will end the process forcefully
		Environment.Exit(error_code);
	}

	/// <summary>
	/// KillProcess forcefully kills the current process after a short delay.
	/// </summary>
	private static void KillProcess()
	{
		Thread.Sleep(1);
		Process.GetCurrentProcess().Kill();
	}
}