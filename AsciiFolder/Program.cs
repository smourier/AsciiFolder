using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

namespace AsciiFolder;

internal class Program
{
    static int _indent;
    static bool _noFiles;
    static bool _filesSize;
    static int _maxDepth;
    static bool _filesLastWriteTime;
    static string _directoriesSearchPattern = null!;
    static string _filesSearchPattern = null!;
    static ConsoleColor _folderColor;
    static SortByComparer _sortByComparer = null!;
    static readonly EnumerationOptions _options = new()
    {
        RecurseSubdirectories = false,
        IgnoreInaccessible = true,
        ReturnSpecialDirectories = false
    };

    static void Main()
    {
        if (Debugger.IsAttached)
        {
            SafeMain();
            return;
        }

        try
        {
            SafeMain();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    static void SafeMain()
    {
        Console.WriteLine($"AsciiFolder v{Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion} - Copyright (C) 2024-" + DateTime.Now.Year + " Simon Mourier. All rights reserved.");
        Console.WriteLine();

        var inputPath = CommandLine.Current.GetNullifiedArgument(0) ?? Path.GetFullPath(".");
        if (CommandLine.Current.HelpRequested || inputPath == null)
        {
            Help();
            return;
        }

        _indent = Math.Max(1, CommandLine.Current.GetArgument("i", 4));
        _maxDepth = CommandLine.Current.GetArgument<int?>("md") ?? int.MaxValue;
        _noFiles = CommandLine.Current.GetArgument<bool>("nf");
        _filesSize = CommandLine.Current.GetArgument<bool>("fs");
        _filesLastWriteTime = CommandLine.Current.GetArgument<bool>("fd");
        _options.AttributesToSkip = CommandLine.Current.GetArgument("as", FileAttributes.System | FileAttributes.Hidden);
        _directoriesSearchPattern = CommandLine.Current.GetNullifiedArgument("dsp") ?? "*";
        _filesSearchPattern = CommandLine.Current.GetNullifiedArgument("fsp") ?? "*";
        _folderColor = CommandLine.Current.GetArgument("fc", ConsoleColor.Yellow);
        if (_folderColor == ConsoleColor.Black)
        {
            _folderColor = ConsoleColor.Yellow;
        }

        var sortDirection = CommandLine.Current.GetArgument("s", ListSortDirection.Ascending);
        var sortBy = CommandLine.Current.GetArgument("sb", SortBy.Name);
        _sortByComparer = new SortByComparer(sortBy, sortDirection);

        DumpFolder(0, inputPath, string.Empty, true, !_noFiles);
    }

    static void DumpFolder(int depth, string path, string indent, bool isLast, bool showFiles)
    {
        if (depth >= _maxDepth)
            return;

        var dir = new DirectoryInfo(path);

        Console.Write(indent);
        Console.Write(isLast ? "└── " : "├── ");
        Console.ForegroundColor = _folderColor;
        Console.WriteLine(dir.Name);
        Console.ResetColor();

        indent += isLast ? " " : "│";
        indent += new string(' ', _indent - 1);

        var subDirs = dir.GetDirectories(_directoriesSearchPattern, _options);
        subDirs.Sort(_sortByComparer);

        var files = showFiles ? dir.GetFiles(_filesSearchPattern, _options) : [];
        files.Sort(_sortByComparer);

        for (var i = 0; i < subDirs.Length; i++)
        {
            var isLastDir = i == subDirs.Length - 1 && files.Length == 0;
            DumpFolder(depth + 1, subDirs[i].FullName, indent, isLastDir, showFiles);
        }

        if (showFiles)
        {
            for (var i = 0; i < files.Length; i++)
            {
                var isLastFile = i == files.Length - 1;

                Console.Write(indent);
                Console.Write(isLastFile ? "└── " : "├── ");
                Console.WriteLine(files[i].Name);

                if (_filesSize || _filesLastWriteTime)
                {
                    var sizeIndent = indent + (isLastFile ? " " : "│");
                    sizeIndent += new string(' ', _indent);
                    Console.Write(sizeIndent);

                    if (_filesSize)
                    {
                        var sizeStr = Conversions.FormatByteSize(files[i].Length);
                        Console.Write(sizeStr);
                    }

                    if (_filesLastWriteTime)
                    {
                        var timeStr = files[i].LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                        if (_filesSize)
                        {
                            Console.Write(" ── ");
                        }
                        Console.Write(timeStr);
                    }
                    Console.WriteLine();
                }
            }
        }
    }

    sealed class SortByComparer(SortBy sortBy, ListSortDirection direction) : IComparer<FileSystemInfo>
    {
        public int Compare(FileSystemInfo? x, FileSystemInfo? y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return direction == ListSortDirection.Ascending ? -1 : 1;

            if (y == null)
                return direction == ListSortDirection.Ascending ? 1 : -1;

            var cmp = sortBy switch
            {
                SortBy.Size => (x is FileInfo fx ? fx.Length : 0).CompareTo(y is FileInfo fy ? fy.Length : 0),
                SortBy.Extension => x.Extension.CompareTo(y.Extension),
                SortBy.LastWriteTime => x.LastWriteTime.CompareTo(y.LastWriteTime),
                SortBy.CreationTime => x.CreationTime.CompareTo(y.CreationTime),
                SortBy.LastAccessTime => x.LastAccessTime.CompareTo(y.LastAccessTime),
                //case SortBy.Name:
                _ => x.Name.CompareTo(y.Name),
            };
            return direction == ListSortDirection.Ascending ? cmp : -cmp;
        }
    }

    static void Help()
    {
        var tabIndent = 5;
        using var iw = new IndentedTextWriter(Console.Out, new string(' ', tabIndent));
        iw.WriteLine("Format:");
        iw.WriteLine();
        iw.WriteLine(Assembly.GetEntryAssembly()!.GetName().Name + " <input path> [options]");
        iw.WriteLine();
        iw.WriteLine("Description:");
        iw.Indent++;
        iw.WriteLine("This tool dumps a given folder as an ASCII tree.");
        iw.Indent--;
        iw.WriteLine();
        iw.WriteLine("Options:");
        iw.Indent++;
        iw.WriteLine("/i   Indentation size (default: 4).");
        iw.WriteLine("/md  Maximum depth to recurse (default: no limit).");
        iw.WriteLine("/nf  Don't output files.");
        iw.WriteLine("/fs  Display files size");
        iw.WriteLine("/fd  Display files last write time");
        iw.WriteLine("/dsp Directories search pattern (default: *).");
        iw.WriteLine("/fsp Files search pattern (default: *).");
        iw.WriteLine("/fc  Folder color (default: Yellow)."); OutputEnumValues<ConsoleColor>(iw, tabIndent, [ConsoleColor.Black], v =>
        {
            Console.ForegroundColor = v;
            iw.Write(v);
            Console.ResetColor();
        });
        iw.WriteLine("/s   Sort direction. (default: Ascending)."); OutputEnumValues<ListSortDirection>(iw, tabIndent);
        iw.WriteLine("/sb  Sort by (default: Name)."); OutputEnumValues<SortBy>(iw, tabIndent);
        iw.WriteLine("/as  File system attributes to skip (default: System, Hidden)."); OutputEnumValues<FileAttributes>(iw, tabIndent, [FileAttributes.None]);
        iw.WriteLine();
    }

    static void OutputEnumValues<T>(IndentedTextWriter iw, int tabIndent, T[]? excluded = null, Action<T>? write = null) where T : struct, Enum
    {
        write ??= v => iw.Write(v.ToString());

        iw.Indent++;
        var token = "Possible values: ";
        iw.Write(token);
        var indent = iw.Indent;
        var consoleMaxSize = Console.WindowWidth;
        var values = Enum.GetValues<T>();
        var lineLength = (iw.Indent * tabIndent) + token.Length;
        var any = false;
        for (var i = 0; i < values.Length; i++)
        {
            if (excluded != null && excluded.Contains(values[i]))
                continue;

            if (any)
            {
                if (lineLength + 2 > consoleMaxSize)
                {
                    iw.WriteLine();
                    lineLength = (iw.Indent * tabIndent) + token.Length;
                }

                iw.Write(", ");
                lineLength += 2;
            }

            lineLength += values[i].ToString().Length;
            if (lineLength > consoleMaxSize)
            {
                iw.WriteLine();
                lineLength = (iw.Indent * tabIndent) + token.Length;
            }

            write(values[i]);
            any = true;
        }
        iw.Indent = indent;
        iw.Indent--;
        iw.WriteLine();
    }

    enum SortBy
    {
        Name,
        Extension,
        Size,
        LastWriteTime,
        CreationTime,
        LastAccessTime
    }
}
