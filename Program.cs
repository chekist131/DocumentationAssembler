using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DocumentationAssembler
{
    /// <summary>
    /// Глава или раздел
    /// </summary>
    public abstract class Node 
    {
        /// <summary>
        /// Заголовок
        /// </summary>
        public string title;
        /// <summary>
        /// Номер внутри раздела
        /// </summary>
        public int number;
        /// <summary>
        /// Уровень заголовка (для разметки)
        /// </summary>
        public int titleLevel;
        /// <summary>
        /// Полный номер заголовка (пр. 3.1.4)
        /// </summary>
        public List<int> titlePosition;
        /// <summary>
        /// Путь
        /// </summary>
        public string path;
    }

    /// <summary>
    /// Глава
    /// </summary>
    public class Paragraph : Node
    {
        /// <summary>
        /// Текст
        /// </summary>
        public string[] text;
        public Paragraph(string title, int number, string[] text, string path)
        {
            this.title = title;
            this.number = number;
            this.text = text;
        }
    }

    /// <summary>
    /// Раздел
    /// </summary>
    public class Section : Node
    {
        /// <summary>
        /// Список подразделов
        /// </summary>
        public List<Node> data;
        public Section(string title, int number, List<Node> data, string path)
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
            MarkingUpTitleLevel(docs, new Stack<int>());
            File.WriteAllLines(FullDocumentationFile, assamble(docs), Encoding.UTF8);
        }

        /// <summary>
        /// Считать из папки в дерево
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <returns></returns>
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
                        File.ReadAllLines(path),
                        path);
                });
            data.AddRange(fileNamesInfo);

            var dirNamesInfo = Directory.GetDirectories(rootFolder)
                .Where(path => { int _res; return int.TryParse(path.Split(new char[] { '\\' }).Last().Split(new char[] { '.' }).First(), out _res); })
                .Select(path =>
                {
                    string[] parts = path.Split(new char[] { '\\' }).Last().Split(new char[] { '.' });
                    return new Section(
                        string.Concat(parts.Skip(1).Take(parts.Length - 1)),
                        int.Parse(parts.First()),
                        makeStructure(path),
                        path);
                });
            data.AddRange(dirNamesInfo);
            return data.OrderBy(e => e.number).ToList();
        }

        /// <summary>
        /// Размеить уровни заголовков их полные номера
        /// </summary>
        /// <param name="l"></param>
        /// <param name="depth">Глубина</param>
        /// <param name="recTitleRoute">Положить new Stack<int>()</int></param>
        public static void MarkingUpTitleLevel(List<Node> l, Stack<int> recTitleRoute, int depth=1)
        {
            foreach(Node n in l)
            {
                recTitleRoute.Push(n.number);
                n.titleLevel = depth;
                n.titlePosition = recTitleRoute.Reverse().ToList();
                if (n is Section)
                {
                    MarkingUpTitleLevel((n as Section).data, recTitleRoute, depth + 1);
                }
                recTitleRoute.Pop();
            }
        }

        /// <summary>
        /// Преобразовать дерево в список строк
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static List<string> assamble(List<Node> l)
        {
            List<string> outDoc = new List<string>();
            foreach(Node n in l)
            {
                outDoc.Add(TitleRoute(n) + " " + n.title + "(debug_title_level=" + n.titleLevel + ")");
                if (n is Paragraph)
                {
                    outDoc.AddRange((n as Paragraph).text);
                    outDoc.Add(string.Empty);
                }
                else if (n is Section)
                {
                    outDoc.AddRange(assamble((n as Section).data));
                }
            }
            return outDoc;
        }

        /// <summary>
        /// Вывод полного номера главы или раздела
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
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
