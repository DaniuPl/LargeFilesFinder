using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{

    class FileSystemEntry
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public bool IsDirectory { get; set; }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Enter Disk Path (np. C:\\):");
        string rootPath = Console.ReadLine();

        if (Directory.Exists(rootPath))
        {
            List<FileSystemEntry> entries = new List<FileSystemEntry>();

            try
            {
                Console.WriteLine("Scan Disk...");

                // Rekurencyjne skanowanie folderów i plików
                ScanDirectory(rootPath, entries);

                // Sortowanie według rozmiaru malejąco
                var largestEntries = entries.OrderByDescending(e => e.Size).Take(10);

                Console.WriteLine("\nLargest files and folders:");
                foreach (var entry in largestEntries)
                {
                    Console.WriteLine($"{(entry.IsDirectory ? "Dir" : "File")}: {entry.Path} - {FormatSize(entry.Size)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while scanning: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("The specified path does not exist.");
        }
    }

    static void ScanDirectory(string path, List<FileSystemEntry> entries)
    {
        try
        {
            foreach (var file in Directory.GetFiles(path))
            {
                UpdateProgress(file);
                try
                {
                    FileInfo fileInfo = new FileInfo(file);
                    entries.Add(new FileSystemEntry
                    {
                        Path = fileInfo.FullName,
                        Size = fileInfo.Length,
                        IsDirectory = false
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"\nNo access to file: {file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                }
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                UpdateProgress(dir);
                try
                {
                    long folderSize = GetDirectorySize(dir);
                    entries.Add(new FileSystemEntry
                    {
                        Path = dir,
                        Size = folderSize,
                        IsDirectory = true
                    });

                    ScanDirectory(dir, entries);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"\nNo access to dir: {dir}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"\nNo access to dir: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError processing folder {path}: {ex.Message}");
        }
    }

    static long GetDirectorySize(string folderPath)
    {
        long totalSize = 0;
        try
        {
            totalSize = Directory.GetFiles(folderPath)
                                 .Select(f => new FileInfo(f).Length)
                                 .Sum();
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"\nNo access to dir: {folderPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError while calculating folder size: {ex.Message}");
        }
        return totalSize;
    }

     
    static void UpdateProgress(string currentItemPath)
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write($"Scan: {currentItemPath}");
    }

    static string FormatSize(long size)
    {
        string[] sizeUnits = { "B", "KB", "MB", "GB", "TB" };
        int unitIndex = 0;

        double sizeDouble = size;
        while (sizeDouble >= 1024 && unitIndex < sizeUnits.Length - 1)
        {
            sizeDouble /= 1024;
            unitIndex++;
        }

        return $"{sizeDouble:0.##} {sizeUnits[unitIndex]}";
    }
}
