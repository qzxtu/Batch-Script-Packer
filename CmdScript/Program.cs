using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CmdScript
{
    internal class Program
	{
        [STAThread]
        private static void Main(string[] args)
        {
            Console.Title = "Batch Script Packer";
            Console.SetWindowSize(60, 15);
            Console.BufferWidth = (Console.WindowWidth = 60);
            Console.BufferHeight = Console.WindowHeight;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" Batch Script Packer");
            Console.WriteLine(" ───────────────────");
            Console.ForegroundColor = ConsoleColor.White;

            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Title = "Select batch script to pack",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "bat",
                Filter = "Batch files (*.bat)|*.bat|Batch files (*.cmd)|*.cmd",
                FilterIndex = 1,
                RestoreDirectory = true,
                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            string batchFilePath = string.Empty;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                batchFilePath = fileDialog.FileName;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" No file selected. Exiting.");
                Console.ResetColor();
                Environment.Exit(0);
            }

            Console.WriteLine("\n Reading batch file...");
            string batchContent = File.ReadAllText(batchFilePath);

            Console.WriteLine(" Creating C# code file...");
            string cSharpContent = $@"using System;
                                    using System.IO;
                                    using System.Diagnostics;

                                    namespace BatchScriptPacker
                                    {{
                                        class Program
                                        {{
                                            static void Main(string[] args)
                                            {{
                                                if (!Directory.Exists(@""C:\temp""))
                                                {{
                                                    Directory.CreateDirectory(@""C:\temp"");
                                                }}
                                                using (StreamWriter sw = File.CreateText(@""C:\temp\temp.bat""))
                                                {{
                                                    sw.WriteLine(@""{batchContent}"");
                                                }}
                                                Process p = new Process()
                                                {{
                                                    StartInfo = new ProcessStartInfo(@""C:\temp\temp.bat"")
                                                }};
                                                p.Start();
                                                p.WaitForExit();
                                                File.Delete(@""C:\temp\temp.bat"");
                                                Directory.Delete(@""C:\temp"");
                                            }}
                                        }}
                                    }}";
            File.WriteAllText("memory.temp", cSharpContent);

            Console.WriteLine(" Compiling...");
            string outputAssembly = Path.GetFileNameWithoutExtension(batchFilePath) + ".exe";
            string arguments = $"/target:winexe /out:\"{outputAssembly}\" \"memory.temp\"";
            Process compiler = new Process
            {
                StartInfo = new ProcessStartInfo("csc.exe", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            compiler.Start();
            Console.WriteLine(compiler.StandardOutput.ReadToEnd());
            compiler.WaitForExit();
            File.Delete("memory.temp");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n Success! File saved as \"{outputAssembly}\"");
            Console.ResetColor();
        }
    }
}