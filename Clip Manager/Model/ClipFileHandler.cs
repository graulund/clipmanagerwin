using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Clip_Manager.Model
{
	public class ClipFileHandler
	{
		public static void WriteClipsFile(Dictionary<int, CachedSound> clips, string file) {
			var directory = Path.GetDirectoryName(file);
			var fileNames = clips
				.ToDictionary(p => p.Key, p => GetRelativePath(directory, p.Value.FileName));

			Console.WriteLine("Writing dictionary: {0}", fileNames);
			WriteDict(fileNames, file);
		}

		public static Dictionary<int, CachedSound> ReadClipsFile(string file) {
			var directory = Path.GetDirectoryName(file);
			Dictionary<int, string> fileNames;

			try {
				fileNames = ReadDict(file);
			}

			catch (Exception e) {
				Console.WriteLine("Exception occurred retrieving clips file: {0}", e.Message);
				return new Dictionary<int, CachedSound>();
			}

			var output = new Dictionary<int, CachedSound>(ClipManagerEngine.NUM_CLIPS);

			foreach (var kv in fileNames) {
				if (kv.Key >= ClipManagerEngine.NUM_CLIPS) {
					continue;
				}
				
				CachedSound clip;

				try {
					var fullFileName = GetFullPath(kv.Value, directory);
					clip = new CachedSound(fullFileName);
				}

				catch (Exception e) {
					Console.WriteLine("Exception occurred loading clip from file: {0}", e.Message);
					continue;
				}

				output.Add(kv.Key, clip);
			}

			return output;
		}

		public static void WriteDict(Dictionary<int, string> dictionary, string file)
		{
			using (FileStream fs = File.OpenWrite(file))
			using (BinaryWriter writer = new BinaryWriter(fs))
			{
				// Put count.
				writer.Write(dictionary.Count);
				// Write pairs.
				foreach (var pair in dictionary)
				{
					writer.Write(pair.Key);
					writer.Write(pair.Value);
				}
			}
		}

		public static Dictionary<int, string> ReadDict(string file)
		{
			var result = new Dictionary<int, string>();
			using (FileStream fs = File.OpenRead(file))
			using (BinaryReader reader = new BinaryReader(fs))
			{
				// Get count.
				int count = reader.ReadInt32();
				// Read in all pairs.
				for (int i = 0; i < count; i++)
				{
					var key = reader.ReadInt32();
					var value = reader.ReadString();
					result[key] = value;
				}
			}
			return result;
		}

		public static string GetFullPath(string filePath, string relativeDirectory) {
			if (Path.IsPathRooted(filePath)) {
				return filePath;
			}

			return Path.GetFullPath(Path.Combine(relativeDirectory, filePath));
		}

		// https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path

		/// <summary>
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="fromPath"/> or <paramref name="toPath"/> is <c>null</c>.</exception>
		/// <exception cref="UriFormatException"></exception>
		/// <exception cref="InvalidOperationException"></exception>

		public static string GetRelativePath(string fromPath, string toPath)
		{
			if (string.IsNullOrEmpty(fromPath)) {
				throw new ArgumentNullException("fromPath");
			}

			if (string.IsNullOrEmpty(toPath)) {
				throw new ArgumentNullException("toPath");
			}

			Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
			Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

			if (fromUri.Scheme != toUri.Scheme) {
				return toPath;
			}

			Uri relativeUri = fromUri.MakeRelativeUri(toUri);
			string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase)) {
				relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return relativePath;
		}

		private static string AppendDirectorySeparatorChar(string path)
		{
			// Append a slash only if the path is a directory and does not have a slash.
			if (!Path.HasExtension(path) &&
				!path.EndsWith(Path.DirectorySeparatorChar.ToString())) {
				return path + Path.DirectorySeparatorChar;
			}

			return path;
		}
	}
}
