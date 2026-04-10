# AsciiFolder
A tool that generates an "ascii tree" from a local folder.md (well, technically, it does support alll Unicode characters 🤭).

# Example
When running this command (md means maximum depth):

    AsciiFolder AsciiFolder /md:2

Here is the result:

    └── AsciiFolder
        ├── AsciiFolder
        │   ├── AsciiFolder.csproj
        │   ├── AsciiFolder.csproj.user
        │   ├── AsciiFolder.ico
        │   └── Program.cs
        ├── .gitignore
        ├── AsciiFolder.slnx
        ├── LICENSE
        └── README.md

It can also output colors:

<img width="505" height="382" alt="AsciiFolder" src="https://github.com/user-attachments/assets/09079b2c-97f0-4fe5-8a1f-2500b5047600" />


# Options
    /i   Indentation size (default: 4).
    /md  Maximum depth to recurse (default: no limit).
    /nf  Don't output files.
    /fs  Display files size
    /fd  Display files last write time
    /dsp Directories search pattern (default: *).
    /fsp Files search pattern (default: *).
    /dep Directories exclude patterns. Use ; as a separator for multiple patterns.
    /fep Files exclude patterns. Use ; as a separator for multiple patterns.
    /fc  Folder color (default: Yellow).
         Possible values: DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta, DarkYellow, Gray, DarkGray, Blue, Green
        , Cyan, Red, Magenta, Yellow, White
    /s   Sort direction. (default: Ascending).
         Possible values: Ascending, Descending
    /sb  Sort by (default: Name).
         Possible values: Name, Extension, Size, LastWriteTime, CreationTime, LastAccessTime
    /as  File system attributes to skip (default: System, Hidden).
         Possible values: ReadOnly, Hidden, System, Directory, Archive, Device, Normal, Temporary, SparseFile,
         ReparsePoint, Compressed, Offline, NotContentIndexed, Encrypted, IntegrityStream, NoScrubData
