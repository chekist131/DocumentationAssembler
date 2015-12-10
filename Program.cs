using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DocumentationAssembler
{
    public abstract class Node 
    {
        public string title;
        public int number;
        public int titleLevel;
        public List<int> titlePosition;
    }

    public class Paragraph : Node
    {
        
        public string[] text;
        public Paragraph(string title, int number, string[] text)
        {
            this.title = title;
            this.number = number;
            this.text = text;
        }
    }

    public class Section : Node
    {
        public List<Node> data;
        public Section(string title, int number, List<Node> data)
        {
            this.title = title;
            this.number = number;
            this.data = data;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string rootFolder = "Documentation";
            string FullDocumentationFile = "OptimizedCompilersProjectDocumentation.txt";
            Directory.CreateDirectory(rootFolder);
            List<Node> docs = makeStructure(rootFolder);
            int depth = Depth(docs);
            MarkingUpTitleLevel(docs, depth, new Stack<int>());
            File.WriteAllLines(FullDocumentationFile, assamble(docs), Encoding.UTF8);
        }

        private static List<Node> makeStructure(string rootFolder)
        {
            List<Node> data = new List<Node>();
            var fileNamesInfo = Directory.GetFiles(rootFolder)
                .Where(path => { int _res; return int.TryParse(Path.GetFileName(path).Split(new char[] { '.' }).First(), out _res); })
                .Select(path =>
                {
                    string[] parts = Path.GetFileName(path).Split(new char[] { '.' });
                    return new Paragraph(
                        string.Concat(parts.Skip(1).Take(parts.Length - 2)),
                        int.Parse(parts.First()),
                        File.ReadAllLines(path));
                });
            data.AddRange(fileNamesInfo);

            var dirNamesInfo = Directory.GetDirectories(rootFolder)
                .Where(path => { int _res; return int.TryParse(path.Split(new char[] {'\\'}).Last().Split(new char[] { '.' }).First(), out _res); })
                .Select(path =>
                {
                    string[] parts = path.Split(new char[] {'\\'}).Last().Split(new char[] { '.' });
                    return new Section(
                        string.Concat(parts.Skip(1).Take(parts.Length - 1)),
                        int.Parse(parts.First()),
                        makeStructure(path));
                });
            data.AddRange(dirNamesInfo);
            return data.OrderBy(e => e.number).ToList();
        }

        public static int Depth(List<Node> l)
        {
            if (l.All(e => e is Paragraph))
                return 1;
            return l.Where(e => e is Section).Select(e => Depth((e as Section).data) + 1).Max();
        }

        public static void MarkingUpTitleLevel(List<Node> l, int depth, Stack<int> recTitleRoute)
        {
            foreach(Node n in l)
            {
                recTitleRoute.Push(n.number);
                n.titleLevel = depth;
                n.titlePosition = recTitleRoute.Reverse().ToList();
                if (n is Section)
                {
                    MarkingUpTitleLevel((n as Section).data, depth - 1, recTitleRoute);
                }
                recTitleRoute.Pop();
            }
        }

        public static List<string> assamble(List<Node> l)
        {
            List<string> outDoc = new List<string>();
            foreach(Node n in l)
            {
                outDoc.Add(TitleRoute(n) + " " + n.title + "(debug_title_level=" + n.titleLevel + ")");
                if (n is Paragraph)
                {
                    outDoc.AddRange((n as Paragraph).text);
                }
                else if (n is Section)
                {
                    outDoc.AddRange(assamble((n as Section).data));
                }
                outDoc.Add(string.Empty);
            }
            return outDoc;
        }

        public static string TitleRoute(Node n)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int i in n.titlePosition)
                sb.Append(i + ".");
            if (sb.Length > 0)
                sb.Length--;
            return sb.ToString();
        }
    }
}
