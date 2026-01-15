#if TOOLS
using System.IO;
using TextualGraph.Serialization;

namespace TextualGraph.Samples;

public class JsonArrayFragmentWriter : FragmentWriter
{
    public override string Id => "json_array";
    
    public override void Begin(TextWriter writer)
    {
        writer.WriteLine("[");
    }

    public override void WriteFragment(TextWriter writer, string fragment, bool isLast)
    {
        if (string.IsNullOrEmpty(fragment))
        {
            writer.WriteLine();
        }
        else
        {
            // 按行分割 fragment（保留换行符风格）
            using var reader = new StringReader(fragment);
            string line;
            bool firstLine = true;
            while ((line = reader.ReadLine()) != null)
            {
                if (!firstLine)
                    writer.WriteLine(); // 写入上一行的换行（因为 ReadLine 去掉了 \n）
                writer.Write("    ");   // 添加 2 空格缩进
                writer.Write(line);
                firstLine = false;
            }
        }

        if (!isLast)
            writer.Write(",");
        writer.WriteLine(); // 最终换行
    }

    public override void End(TextWriter writer)
    {
        writer.WriteLine("]");
    }
}

#endif