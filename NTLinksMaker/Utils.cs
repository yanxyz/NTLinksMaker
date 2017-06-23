using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace NTLinksMaker
{
    public class Utils
    {
        public static void CreateLink(string link, string target, LinkType linkType, bool relative = false, bool force = false)
        {
            target = target.TrimEnd(new[] { '\\', ' ' });
            if (Path.GetDirectoryName(target) == target)
                throw new Exception("Target should not be root.");

            if (String.IsNullOrEmpty(link))
                link = Path.GetFileName(target);
            if (String.IsNullOrEmpty(link))
                throw new Exception("Could not get file name of Target.");

            if (File.Exists(link))
            {
                if (force)
                    File.Delete(link);
                else
                    throw new Exception("Link already exsits.");
            }

            if (Directory.Exists(link))
            {
                if (force)
                    Directory.Delete(link);
                else
                    throw new Exception("Link already exsits.");
            }

            var opt = string.Empty;
            var t = linkType.Value;
            var k = GetPathType(target);
            var notFile = "Target is not a file.";
            var notDir = "Target is not a directory.";

            if (t == 0)
            {
                switch (k)
                {
                    case 0:
                        throw new Exception("Target is not found.");
                    case 2:
                        throw new Exception(notFile);
                    default:
                        if (!IsSameVolume(link, target))
                            throw new Exception("Link and target is not on the same volume.");
                        break;
                }
            }
            else if (t == 1)
            {
                if (k == 1)
                    throw new Exception(notDir);
            }
            else
            {
                if (k == 2 && t == 2)
                    throw new Exception(notFile);

                if (k == 1 && t == 3)
                    throw new Exception(notDir);

                if (relative && IsAbsolutePath(target))
                    target = GetRelativePath(target, link);
            }

            // mklink is a internal command
            var arguments = $"/c mklink {linkType.Switch} \"{link}\" \"{target}\"";

            if (t > 1 && !App.PreferSymlink)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = App.ExecutablePath,
                    Verb = "RunAs",
                    Arguments = "/admin " + arguments
                };
                Process.Start(startInfo);
                return;
            }
            else
            {
                RunCmd(arguments);
            }
        }

        public static void CreateLinkByAdmin(string[] args)
        {
            var sb = new StringBuilder();
            for (var i = 1; i < args.Length; i++)
            {
                var item = args[i];
                if (item.IndexOf(" ") > -1)
                {
                    sb.Append("\"");
                    sb.Append(item);
                    sb.Append("\" ");
                }
                else
                {
                    sb.Append(item);
                    sb.Append(" ");
                }
            }

            RunCmd(sb.ToString());
        }

        private static void RunCmd(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = arguments,
                RedirectStandardError = true
            };
            var proc = Process.Start(startInfo);
            var error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            if (proc.ExitCode > 0)
                throw new Exception(error);
        }

        public static bool DeveloperModeEnabled()
        {
            try
            {
                using (var localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
                {
                    var value = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock").GetValue("AllowDevelopmentWithoutDevLicense");
                    return Convert.ToUInt32(value) == 1;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool MaybeDirectory(string path)
        {
            return String.IsNullOrEmpty(Path.GetExtension(path));
        }

        private static int GetPathType(string path)
        {
            if (File.Exists(path)) return 1;
            if (Directory.Exists(path)) return 2;
            return 0;
        }

        private static bool IsSameVolume(string path1, string path2)
        {
            var a = Path.GetPathRoot(path1);
            var b = Path.GetPathRoot(path2);
            Debug.WriteLine($"{a}, {b}");
            if (a == String.Empty) return a == b;
            if (a[1] == ':') return a.ToLower() == b.ToLower();
            return a == b;
        }

        public static bool IsAbsolutePath(string path)
        {
            return !String.IsNullOrEmpty(Path.GetPathRoot(path));
        }

        public static string GetRelativePath(string fromPath, string toPath)
        {
            fromPath = Path.GetFullPath(fromPath);
            // handle for place other than CreateLink() 
            if (String.IsNullOrEmpty(toPath))
                toPath = Path.GetFileName(fromPath);
            toPath = Path.GetFullPath(toPath);

            var s = new[] { '\\' };
            var partsA = fromPath.Split(s, StringSplitOptions.RemoveEmptyEntries);
            var partsB = toPath.Split(s, StringSplitOptions.RemoveEmptyEntries);
            var lengthA = partsA.Length;
            var lengthB = partsB.Length;

            // step 1: get common parts
            var k = -1;
            var min = lengthA < lengthB ? lengthA : lengthB;
            for (var n = 0; n < min; n++)
            {
                if (partsA[n].Equals(partsB[n], StringComparison.OrdinalIgnoreCase))
                    ++k;
                else
                    break;
            }

            // step 2: resolve relative path
            if (k == -1) return toPath;
            var sb = new StringBuilder();
            for (var i = k + 1; i < lengthA; i++)
            {
                sb.Append("..\\");
            }
            for (var i = k + 1; i < lengthB - 1; i++)
            {
                sb.Append(partsB[i] + "\\");
            }
            sb.Append(partsB[lengthB - 1]);
            return sb.ToString();
        }
    }
}
