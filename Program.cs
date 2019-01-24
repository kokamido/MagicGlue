using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace MagicGlue
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var currentDir = Directory.GetCurrentDirectory();
            
            var outFilePath = Path.Combine(currentDir,"result.cs");
            if(File.Exists(outFilePath))
                File.Delete(outFilePath);
            
            var exceptionsFilePath = Path.Combine(currentDir,"exceptions");
            var exceptions = new HashSet<string>();
            if(File.Exists(exceptionsFilePath))
                exceptions = new HashSet<string>(File.ReadAllLines(exceptionsFilePath).Select(s => Path.Combine(currentDir,s)));
            var res = GetFiles(currentDir,exceptions)
                .Where(f => f.EndsWith(".cs") && !exceptions.Contains(f))
                .Select(ProcessFile)
                .Aggregate((new StringBuilder(), new StringBuilder("namespace CodinGame\r\n{\r\n")), (acc, data) =>
                {
                    acc.Item1.Append(data.usings);
                    acc.Item2.Append(data.code);
                    return acc;
                });
            res.Item2.Append("\n}");       
            var usings = string.Join("\r\n",res.Item1.ToString().Split(new[] {'\r', '\n'}).Distinct());
            File.WriteAllLines(outFilePath,new []{usings, res.Item2.ToString()});

        }

        private static IEnumerable<string> GetFiles(string path, HashSet<string> exceptions)
        {
            var res = new List<string>();
            GetFiles(path, exceptions, res);
            foreach (var re in res)
            {
                Console.WriteLine(re);
            }
            return res;
        }

        private static void GetFiles(string directory, HashSet<string> exceptions, List<string> files)
        {
            files.AddRange(Directory.GetFiles(directory));
            foreach (var dir in Directory.GetDirectories(directory))
            {
                if(!exceptions.Contains(dir))
                    GetFiles(dir, exceptions, files);
            }
        }

        private static (StringBuilder usings, StringBuilder code) ProcessFile(string path)
        {
            int brackets = 0;
            var usings = new StringBuilder();
            var code = new StringBuilder();
            bool isUsing = true;
            foreach (var line in File.ReadAllLines(path))
            {
                if (isUsing)
                {
                    if (line.Contains("using "))
                    {
                        usings.Append(line);
                        usings.Append("\r\n");
                    }
                    else
                        isUsing = false;
                }
                else
                {
                    if (line.Contains("namespace "))
                        continue;
                    if(brackets>0)
                        code.Append(line);
                    brackets += line.Aggregate(0, (acc, c) =>
                    {
                        if (c == '{')
                            acc++;
                        if (c == '}')
                            acc--;
                        return acc;
                    });

                    if (brackets == 0)
                    {
                        code.Length--;
                        return (usings, code);
                    }
                    
                    code.Append("\r\n");
                }
            }
            throw new ArgumentException("Seems like incorrect .cs file");
        }
    }
}